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

    private IQueryable<LevelPlaylistRelation> LevelPlaylistRelationsIncluded => this.LevelPlaylistRelations
        .Include(r => r.Playlist)
        .Include(r => r.Level);

    private IQueryable<SubPlaylistRelation> SubPlaylistRelationsIncluded => this.SubPlaylistRelations
        .Include(r => r.SubPlaylist)
        .Include(r => r.SubPlaylist.Publisher);

    private IQueryable<FavouritePlaylistRelation> FavouritePlaylistRelationsIncluded => this.FavouritePlaylistRelations
        .Include(r => r.Playlist)
        .Include(r => r.Playlist.Publisher);
    
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

    public GamePlaylist CreateRootPlaylist(GameUser user)
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
        return rootPlaylist;
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
            if (this.SubPlaylistRelations.Any(p => p.SubPlaylistId == child.PlaylistId && p.PlaylistId == parent.PlaylistId))
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
                this.SubPlaylistRelations.FirstOrDefault(r => r.SubPlaylistId == child.PlaylistId && r.PlaylistId == parent.PlaylistId);

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
            if (this.LevelPlaylistRelations.Any(p => p.LevelId == level.LevelId && p.PlaylistId == parent.PlaylistId))
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
            this.LevelPlaylistRelations.FirstOrDefault(r => r.LevelId == level.LevelId && r.PlaylistId == parent.PlaylistId);

        if (relation == null)
            return;

        this.Write(() =>
        {
            this.LevelPlaylistRelations.Remove(relation);
        });
    }

    public void ReorderLevelsInPlaylist(IEnumerable<int> levelIds, GamePlaylist parent)
    {
        IEnumerable<LevelPlaylistRelation> relations = this.GetLevelRelationsForPlaylist(parent).ToArray();
        IEnumerable<LevelPlaylistRelation> includedRelations = relations.Where(r => levelIds.Contains(r.LevelId));
        IEnumerable<LevelPlaylistRelation> excludedRelations = relations.Where(r => !levelIds.Contains(r.LevelId));

        this.Write(() => 
        {
            // update playlist levels referenced in the given list
            int newIndex = 0;
            foreach (int levelId in levelIds)
            {
                LevelPlaylistRelation? includedRelation = includedRelations.FirstOrDefault(r => r.LevelId == levelId);

                // only update if the playlist actually contains the level
                if (includedRelation != null)
                {
                    includedRelation.Index = newIndex;
                    newIndex++;
                }
            }

            // update levels not included in the list to retain their previously set order, but to be behind the newly ordered levels
            foreach (LevelPlaylistRelation excludedRelation in excludedRelations)
            {
                excludedRelation.Index = newIndex;
                newIndex++;
            }
        });
    }

    private IEnumerable<LevelPlaylistRelation> GetLevelRelationsForPlaylist(GamePlaylist playlist)
        => this.LevelPlaylistRelationsIncluded
            .Where(r => r.PlaylistId == playlist.PlaylistId)
            .OrderBy(r => r.Index);

    public DatabaseList<GameLevel> GetLevelsInPlaylist(GamePlaylist playlist, TokenGame game, int skip, int count)
        => new(this.GetLevelRelationsForPlaylist(playlist)
            .Select(l => l.Level)
            .FilterByGameVersion(game), skip, count);

    public int GetTotalLevelsInPlaylistCount(GamePlaylist playlist) 
        => this.LevelPlaylistRelations.Count(l => l.Playlist == playlist);

    [Obsolete("Only to be used by GameDatabaseContext and GamePlaylistExtensions.")]
    public IEnumerable<GamePlaylist> GetPlaylistsContainingPlaylist(GamePlaylist playlist)
        => this.SubPlaylistRelations
            .Where(p => p.SubPlaylist == playlist)
            .OrderByDescending(r => r.Timestamp)
            .Select(r => this.GamePlaylists.First(p => p.PlaylistId == r.PlaylistId))
            .Where(p => !p.IsRoot);

#pragma warning disable CS0618 // obsolete warning
    public DatabaseList<GamePlaylist> GetPlaylistsContainingPlaylist(GamePlaylist playlist, int skip, int count)
        => new(GetPlaylistsContainingPlaylist(playlist), skip, count);

    public DatabaseList<GamePlaylist> GetPlaylistsByAuthorContainingPlaylist(GameUser user, GamePlaylist playlist, int skip, int count)
        => new(GetPlaylistsContainingPlaylist(playlist)
            .Where(p => p.PublisherId == user.UserId), skip, count);
#pragma warning restore CS0618

    public DatabaseList<GamePlaylist> GetPlaylistsInPlaylist(GamePlaylist playlist, int skip, int count)
        => new(this.SubPlaylistRelationsIncluded
            .Where(p => p.PlaylistId == playlist.PlaylistId)
            .OrderByDescending(r => r.Timestamp)
            .Select(l => l.SubPlaylist), skip, count);
    
    public DatabaseList<GamePlaylist> GetPlaylistsByAuthor(GameUser author, int skip, int count)
        => new(this.GamePlaylists
            .Where(p => p.PublisherId == author.UserId)
            .Where(p => !p.IsRoot)
            .OrderByDescending(p => p.LastUpdateDate), skip, count);
    
    public IEnumerable<GamePlaylist> GetPlaylistsContainingLevelInternal(GameLevel level)
        => this.LevelPlaylistRelationsIncluded
            .Where(p => p.LevelId == level.LevelId)
            .OrderByDescending(r => r.Timestamp)
            .Select(r => r.Playlist);

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
        => new(this.FavouritePlaylistRelationsIncluded
            .Where(r => r.UserId == user.UserId)
            .OrderByDescending(r => r.Timestamp)
            .Select(r => r.Playlist), skip, count);

    public int GetFavouriteCountForPlaylist(GamePlaylist playlist)
        => this.FavouritePlaylistRelations.Count(r => r.PlaylistId == playlist.PlaylistId);

    public bool IsPlaylistFavouritedByUser(GamePlaylist playlist, GameUser user)
        => this.FavouritePlaylistRelations.Any(r => r.PlaylistId == playlist.PlaylistId && r.UserId == user.UserId);

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
            .FirstOrDefault(r => r.PlaylistId == playlist.PlaylistId && r.UserId == user.UserId);

        if (relation == null) return false;

        this.Write(() => this.FavouritePlaylistRelations.Remove(relation));

        return true;
    }
}