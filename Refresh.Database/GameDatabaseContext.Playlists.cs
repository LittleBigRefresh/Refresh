using Refresh.Database.Models;
using Refresh.Database.Query;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Relations;

namespace Refresh.Database;

public partial class GameDatabaseContext // Playlists
{
    /// <summary>
    /// Default icon used by playlists created in LBP3, through ApiV3 or similar
    /// </summary>
    private const string DefaultPlaylistIcon = "g18451"; // LBP1 star sticker

    private void CreatePlaylistInternal(GamePlaylist createInfo)
    {
        DateTimeOffset now = this._time.Now;
        
        createInfo.CreationDate = now;
        createInfo.LastUpdateDate = now;
        
        this.Write(() =>
        {
            this.GamePlaylists.Add(createInfo);
        });
    }

    public GamePlaylist CreatePlaylist(GameUser user, ISerializedCreatePlaylistInfo createInfo, bool rootPlaylist = false)
    {
        GameLocation location = createInfo.Location ?? GameLocation.Random;
        
        GamePlaylist playlist = new() 
        {
            Publisher = user,
            Name = createInfo.Name ?? "",
            Description = createInfo.Description ?? "",
            IconHash = createInfo.Icon ?? DefaultPlaylistIcon,
            LocationX = location.X,
            LocationY = location.Y,
            IsRoot = rootPlaylist,
        };

        this.CreatePlaylistInternal(playlist);

        return playlist;
    }

    public void CreateRootPlaylist(GameUser user)
    {
        GameLocation randomLocation = GameLocation.Random;

        GamePlaylist rootPlaylist = new()
        {
            Publisher = user,
            Name = "My Playlists",
            Description = $"{user.Username}'s root playlist",
            IconHash = DefaultPlaylistIcon,
            LocationX = randomLocation.X,
            LocationY = randomLocation.Y,
            IsRoot = true,
        };

        this.CreatePlaylistInternal(rootPlaylist);
    }

    public GamePlaylist? GetPlaylistById(int playlistId) 
        => this.GamePlaylists.FirstOrDefault(p => p.PlaylistId == playlistId);

    public void UpdatePlaylist(GamePlaylist playlist, ISerializedCreatePlaylistInfo updateInfo)
    {
        GameLocation location = updateInfo.Location ?? new GameLocation(playlist.LocationX, playlist.LocationY);
        
        this.Write(() =>
        {
            playlist.Name = updateInfo.Name ?? playlist.Name;
            playlist.Description = updateInfo.Description ?? playlist.Description;
            playlist.IconHash = updateInfo.Icon ?? playlist.IconHash;
            playlist.LocationX = location.X;
            playlist.LocationY = location.Y;
            playlist.LastUpdateDate = this._time.Now;
        });
    }

    public void DeletePlaylist(GamePlaylist playlist)
    {
        this.Write(() =>
        {
            // Remove all relations relating to this playlist
            this.LevelPlaylistRelations.RemoveRange(l => l.Playlist == playlist);
            this.SubPlaylistRelations.RemoveRange(l => l.Playlist == playlist || l.SubPlaylist == playlist);
            this.FavouritePlaylistRelations.RemoveRange(l => l.Playlist == playlist);
            
            // Remove the playlist object
            this.GamePlaylists.Remove(playlist);
        });
    }

    public void AddPlaylistToPlaylist(GamePlaylist child, GamePlaylist parent)
    {
        this.Write(() =>
        {
            // Make sure to not create a duplicate object
            if (this.SubPlaylistRelations.Any(p => p.SubPlaylist == child && p.Playlist == parent))
                return;
            
            // Add the relation
            this.SubPlaylistRelations.Add(new SubPlaylistRelation
            {
                Playlist = parent,
                SubPlaylist = child,
                Timestamp = this._time.Now,
            });
        });
    }
    
    public void RemovePlaylistFromPlaylist(GamePlaylist child, GamePlaylist parent)
    {
        this.Write(() =>
        {
            SubPlaylistRelation? relation =
                this.SubPlaylistRelations.FirstOrDefault(r => r.SubPlaylist == child && r.Playlist == parent);

            if (relation == null)
                return;
            
            this.SubPlaylistRelations.Remove(relation);
        });
    }
    
    public void AddLevelToPlaylist(GameLevel level, GamePlaylist parent)
    {
        this.Write(() =>
        {
            // Make sure to not create a duplicate object
            if (this.LevelPlaylistRelations.Any(p => p.Level == level && p.Playlist == parent))
                return;
            
            // Add the relation
            this.LevelPlaylistRelations.Add(new LevelPlaylistRelation
            {
                Level = level,
                Playlist = parent,
                // index of new relation = index of last relation + 1 = relation count (without new relation)
                Index = this.GetTotalLevelsInPlaylistCount(parent),
                Timestamp = this._time.Now,
            });
        });
    }
    
    public void RemoveLevelFromPlaylist(GameLevel level, GamePlaylist parent)
    {
        LevelPlaylistRelation? relation =
            this.LevelPlaylistRelations.FirstOrDefault(r => r.Level == level && r.Playlist == parent);

        if (relation == null)
            return;

        // decrease index of every playlist level after this one by 1
        this.DecreasePlaylistLevelIndicesAfterIndex(parent, relation.Index);

        this.Write(() =>
        {
            this.LevelPlaylistRelations.Remove(relation);
        });
    }

    private void DecreasePlaylistLevelIndicesAfterIndex(GamePlaylist playlist, int startIndex)
    {
        IEnumerable<LevelPlaylistRelation> relations = this.LevelPlaylistRelations
            .Where(r => r.Playlist == playlist && r.Index >= startIndex)
            .AsEnumerable();

        this.Write(() => {
            foreach(LevelPlaylistRelation relation in relations)
            {
                relation.Index--;
            }
        });
    }

    public void UpdatePlaylistLevelOrder(GamePlaylist playlist, List<int> levelIds)
    {
        IEnumerable<LevelPlaylistRelation> relations = this.LevelPlaylistRelations.Where(p => p.Playlist == playlist).OrderBy(r => r.Index);
        int relationCount = relations.Count();

        int newIndex = 0;
        this.Write(() => {
            // overwrite the indices of relations whose level IDs appear in the given list, based on the order of these IDs
            foreach (int levelId in levelIds)
            {
                LevelPlaylistRelation? relation = relations.FirstOrDefault(r => r.Level.LevelId == levelId);
                if (relation != null)
                {
                    relation.Index = newIndex;
                    newIndex++;
                }
            }

            // now "append" the relations whose level IDs did not appear in the given list
            while (newIndex < relationCount)
            {
                relations.ElementAt(newIndex).Index = newIndex;
                newIndex++;
            }
        });
    }
        
    public DatabaseList<GameLevel> GetLevelsInPlaylist(GamePlaylist playlist, TokenGame game, int skip, int count)
        => new(this.LevelPlaylistRelations
            .Where(l => l.Playlist == playlist)
            .OrderBy(r => r.Index)
            .Select(l => l.Level)
            .FilterByGameVersion(game), skip, count);

    public int GetTotalLevelsInPlaylistCount(GamePlaylist playlist) 
        => this.LevelPlaylistRelations.Count(l => l.Playlist == playlist);

    private IEnumerable<GamePlaylist> GetPlaylistsContainingPlaylistInternal(GamePlaylist playlist)
    {
        IQueryable<SubPlaylistRelation> allParents = this.SubPlaylistRelations
            .Where(p => p.SubPlaylist == playlist)
            .OrderByDescending(r => r.Timestamp);
        
        return allParents
            .Select(r => this.GamePlaylists.First(p => p.PlaylistId == r.Playlist.PlaylistId))
            .Where(p => !p.IsRoot);
    }

    public DatabaseList<GamePlaylist> GetPlaylistsContainingPlaylist(GamePlaylist playlist, int skip, int count)
        => new(GetPlaylistsContainingPlaylistInternal(playlist), skip, count);

    public DatabaseList<GamePlaylist> GetPlaylistsByAuthorContainingPlaylist(GameUser user, GamePlaylist playlist, int skip, int count)
        => new(GetPlaylistsContainingPlaylistInternal(playlist)
            .Where(p => p.PublisherId == user.UserId), skip, count);

    public DatabaseList<GamePlaylist> GetPlaylistsInPlaylist(GamePlaylist playlist, int skip, int count)
        => new(this.SubPlaylistRelations
            .Where(p => p.Playlist == playlist)
            .OrderByDescending(r => r.Timestamp)
            .Select(l => l.SubPlaylist), skip, count);
    
    public DatabaseList<GamePlaylist> GetPlaylistsByAuthor(GameUser author, int skip, int count)
        => new(this.GamePlaylists
            .Where(p => p.PublisherId == author.UserId)
            .Where(p => !p.IsRoot)
            .OrderByDescending(p => p.LastUpdateDate), skip, count);
    
    public IEnumerable<GamePlaylist> GetPlaylistsContainingLevelInternal(GameLevel level)
        => this.LevelPlaylistRelations
            .Where(p => p.Level == level)
            .OrderByDescending(r => r.Timestamp)
            .Select(r => this.GamePlaylists.First(p => p.PlaylistId == r.Playlist.PlaylistId));

    public DatabaseList<GamePlaylist> GetPlaylistsByAuthorContainingLevel(GameUser author, GameLevel level, int skip, int count)
        => new(GetPlaylistsContainingLevelInternal(level)
            .Where(p => p.PublisherId == author.UserId), skip, count);
    
    public DatabaseList<GamePlaylist> GetPlaylistsContainingLevel(GameLevel level, int skip, int count)
        => new(GetPlaylistsContainingLevelInternal(level), skip, count);

    public DatabaseList<GamePlaylist> GetNewestPlaylists(int skip, int count)
        => new(this.GamePlaylists
            .Where(p => !p.IsRoot)
            .OrderByDescending(p => p.CreationDate), skip, count);

    public DatabaseList<GamePlaylist> GetMostHeartedPlaylists(int skip, int count) 
        // TODO: reduce code duplication for getting most of x
        => new(this.FavouritePlaylistRelations
            .GroupBy(r => r.Playlist)
            .Select(g => new { Playlist = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Select(x => x.Playlist)
            .Where(p => p != null), skip, count);

    public DatabaseList<GamePlaylist> GetPlaylistsFavouritedByUser(GameUser user, int skip, int count) 
        => new(this.FavouritePlaylistRelations
            .Where(r => r.User == user)
            .OrderByDescending(r => r.Timestamp)
            .Select(r => r.Playlist), skip, count);

    public int GetFavouriteCountForPlaylist(GamePlaylist playlist)
        => this.FavouritePlaylistRelations.Count(r => r.Playlist == playlist);

    public bool IsPlaylistFavouritedByUser(GamePlaylist playlist, GameUser user)
        => this.FavouritePlaylistRelations.FirstOrDefault(r => r.Playlist == playlist && r.User == user) != null;

    public bool FavouritePlaylist(GamePlaylist playlist, GameUser user)
    {
        if (this.IsPlaylistFavouritedByUser(playlist, user)) return false;

        FavouritePlaylistRelation relation = new()
        {
            Playlist = playlist,
            User = user,
            Timestamp = this._time.Now,
        };
        this.Write(() => this.FavouritePlaylistRelations.Add(relation));

        return true;
    }

    public bool UnfavouritePlaylist(GamePlaylist playlist, GameUser user)
    {
        FavouritePlaylistRelation? relation = this.FavouritePlaylistRelations
            .FirstOrDefault(r => r.Playlist == playlist && r.User == user);

        if (relation == null) return false;

        this.Write(() => this.FavouritePlaylistRelations.Remove(relation));

        return true;
    }
}