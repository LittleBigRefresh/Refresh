using Refresh.GameServer.Authentication;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Playlists;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Playlists
{
    /// <summary>
    /// Default icon used by playlists created in LBP3, through ApiV3 or similar
    /// </summary>
    private const string defaultPlaylistIcon = "g18451"; // LBP1 star sticker

    private void CreatePlaylistInternal(GamePlaylist createInfo)
    {
        DateTimeOffset now = this._time.Now;

        this.AddSequentialObject(createInfo, () =>
        {
            createInfo.CreationDate = now;
            createInfo.LastUpdateDate = now;
        });
    }

    public GamePlaylist CreatePlaylist(GameUser user, SerializedLbp1Playlist createInfo, bool rootPlaylist = false)
    {
        GamePlaylist playlist = new() 
        {
            Publisher = user, 
            Name = createInfo.Name,
            Description = createInfo.Description, 
            IconHash = createInfo.Icon, 
            LocationX = createInfo.Location.X, 
            LocationY = createInfo.Location.Y,
            IsRoot = rootPlaylist,
        };

        this.CreatePlaylistInternal(playlist);

        return playlist;
    }

    public GamePlaylist CreatePlaylist(GameUser user, SerializedLbp3Playlist createInfo)
    {
        GameLocation randomLocation = GameLocation.RandomLocation;

        GamePlaylist playlist = new()
        {
            Publisher = user, 
            Name = createInfo.Name ?? "",
            Description = createInfo.Description ?? "", 
            IconHash = defaultPlaylistIcon,
            LocationX = randomLocation.X, 
            LocationY = randomLocation.Y,
            IsRoot = false,
        };

        this.CreatePlaylistInternal(playlist);

        return playlist;
    }

    public void CreateRootPlaylist(GameUser user)
    {
        GameLocation randomLocation = GameLocation.RandomLocation;

        GamePlaylist rootPlaylist = new()
        {
            Publisher = user,
            Name = "My Playlists",
            Description = $"{user.Username}'s root playlist",
            IconHash = defaultPlaylistIcon,
            LocationX = randomLocation.X,
            LocationY = randomLocation.Y,
            IsRoot = true
        };

        this.CreatePlaylistInternal(rootPlaylist);
        this.SetUserRootPlaylist(user, rootPlaylist);
    }

    public GamePlaylist? GetPlaylistById(int playlistId) 
        => this.GamePlaylists.FirstOrDefault(p => p.PlaylistId == playlistId);

    public void UpdatePlaylist(GamePlaylist playlist, SerializedLbp1Playlist updateInfo)
    {
        this.Write(() =>
        {
            playlist.Name = updateInfo.Name;
            playlist.Description = updateInfo.Description;
            playlist.IconHash = updateInfo.Icon;
            playlist.LocationX = updateInfo.Location.X;
            playlist.LocationY = updateInfo.Location.Y;
            playlist.LastUpdateDate = this._time.Now;
        });
    }

    public void UpdatePlaylist(GamePlaylist playlist, SerializedLbp3Playlist updateInfo)
    {
        this.Write(() =>
        {
            if (updateInfo.Name != null) playlist.Name = updateInfo.Name;
            if (updateInfo.Description != null) playlist.Description = updateInfo.Description;
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

    public IEnumerable<GamePlaylist> GetPlaylistsContainingPlaylist(GamePlaylist playlist)
        // TODO: with postgres this can be IQueryable
        => this.SubPlaylistRelations.Where(p => p.SubPlaylist == playlist).OrderByDescending(r => r.Timestamp)
            .AsEnumerable()
            .Select(r => this.GamePlaylists.First(p => p.PlaylistId == r.Playlist.PlaylistId))
            .Where(p => !p.IsRoot);

    public IEnumerable<GamePlaylist> GetPlaylistsByAuthorContainingPlaylist(GameUser user, GamePlaylist playlist)
        // TODO: with postgres this can be IQueryable
        => this.SubPlaylistRelations.Where(p => p.SubPlaylist == playlist).OrderByDescending(r => r.Timestamp)
            .AsEnumerable()
            .OrderByDescending(r => r.Timestamp)
            .Select(r => this.GamePlaylists.First(p => p.PlaylistId == r.Playlist.PlaylistId))
            .Where(p => p.Publisher.UserId == user.UserId)
            .Where(p => !p.IsRoot);

    public IEnumerable<GameLevel> GetLevelsInPlaylist(GamePlaylist playlist)
        // TODO: When we have postgres, remove the `AsEnumerable` call for performance. 
        => this.LevelPlaylistRelations
            .Where(l => l.Playlist == playlist)
            .OrderBy(r => r.Index)
            .AsEnumerable()
            .Select(l => l.Level);

    public IEnumerable<GameLevel> GetLevelsInPlaylist(GamePlaylist playlist, TokenGame game)
        // TODO: When we have postgres, remove the `AsEnumerable` call for performance. 
        => this.LevelPlaylistRelations
            .Where(l => l.Playlist == playlist)
            .OrderBy(r => r.Index)
            .AsEnumerable()
            .Select(l => l.Level)
            .FilterByGameVersion(game);

    public int GetTotalLevelsInPlaylistCount(GamePlaylist playlist, TokenGame game) => 
        this.LevelPlaylistRelations.Where(l => l.Playlist == playlist).AsEnumerable()
            .Select(l => l.Level)
            .FilterByGameVersion(game)
            .Count();

    public int GetTotalLevelsInPlaylistCount(GamePlaylist playlist) => 
        this.LevelPlaylistRelations.Count(l => l.Playlist == playlist);

    public IEnumerable<GamePlaylist> GetPlaylistsInPlaylist(GamePlaylist playlist)
        // TODO: When we have postgres, remove the `AsEnumerable` call for performance. 
        => this.SubPlaylistRelations.Where(p => p.Playlist == playlist).OrderByDescending(r => r.Timestamp)
            .AsEnumerable()
            .Select(l => l.SubPlaylist);

    public IEnumerable<GamePlaylist> GetPlaylistsByAuthor(GameUser author)
        => this.GamePlaylists.Where(p => p.Publisher == author)
            .Where(p => !p.IsRoot)
            .OrderByDescending(p => p.LastUpdateDate);

    public IEnumerable<GamePlaylist> GetPlaylistsByAuthorContainingLevel(GameUser author, GameLevel level)
        // TODO: When we have postgres, remove the `AsEnumerable` call for performance. 
        => this.LevelPlaylistRelations.Where(p => p.Level == level).OrderByDescending(r => r.Timestamp)
            .AsEnumerable()
            .Select(r => this.GamePlaylists.First(p => p.PlaylistId == r.Playlist.PlaylistId))
            .Where(p => p.Publisher.UserId == author.UserId);
    
    public IEnumerable<GamePlaylist> GetPlaylistsContainingLevel(GameLevel level)
        // TODO: When we have postgres, remove the `AsEnumerable` call for performance. 
        => this.LevelPlaylistRelations.Where(p => p.Level == level).OrderByDescending(r => r.Timestamp)
            .AsEnumerable()
            .Select(r => this.GamePlaylists.First(p => p.PlaylistId == r.Playlist.PlaylistId));

    public IEnumerable<GamePlaylist> GetNewestPlaylists()
        => this.GamePlaylists.Where(p => !p.IsRoot)
            .OrderByDescending(p => p.CreationDate);

    public IEnumerable<GamePlaylist> GetMostHeartedPlaylists() 
        // TODO: When we have postgres, remove the `AsEnumerable` call for performance.
        // TODO: reduce code duplication for getting most of x
        => this.FavouritePlaylistRelations
            .GroupBy(r => r.Playlist)
            .Select(g => new { Playlist = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count).AsEnumerable()
            .Select(x => x.Playlist)
            .Where(p => p != null);

    public IEnumerable<GamePlaylist> GetPlaylistsFavouritedByUser(GameUser user) 
        // TODO: When we have postgres, remove the `AsEnumerable` call for performance.
        => this.FavouritePlaylistRelations.Where(r => r.User == user)
            .OrderByDescending(r => r.Timestamp).AsEnumerable()
            .Select(r => r.Playlist);

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