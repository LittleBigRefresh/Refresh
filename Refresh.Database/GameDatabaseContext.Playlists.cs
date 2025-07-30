using Refresh.Database.Models;
using Refresh.Database.Query;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Relations;
using Refresh.Database.Models.Statistics;

namespace Refresh.Database;

public partial class GameDatabaseContext // Playlists
{
    /// <summary>
    /// Default icon used by playlists created in LBP3, through ApiV3 or similar
    /// </summary>
    private const string DefaultPlaylistIcon = "g18451"; // LBP1 star sticker

    private IQueryable<GamePlaylist> GamePlaylistsIncluded => this.GamePlaylists
        .Include(p => p.Statistics)
        .Include(p => p.Publisher);

    private IQueryable<LevelPlaylistRelation> LevelPlaylistRelationsIncluded => this.LevelPlaylistRelations
        .Include(r => r.Playlist)
        .Include(r => r.Playlist.Statistics)
        .Include(r => r.Playlist.Publisher)
        .Include(r => r.Level)
        .Include(r => r.Level.Statistics)
        .Include(r => r.Level.Publisher);

    private IQueryable<SubPlaylistRelation> SubPlaylistRelationsIncluded => this.SubPlaylistRelations
        .Include(r => r.Playlist)
        .Include(r => r.Playlist.Statistics)
        .Include(r => r.Playlist.Publisher)
        .Include(r => r.SubPlaylist)
        .Include(r => r.SubPlaylist.Statistics)
        .Include(r => r.SubPlaylist.Publisher);

    private IQueryable<FavouritePlaylistRelation> FavouritePlaylistRelationsIncluded => this.FavouritePlaylistRelations
        .Include(r => r.Playlist)
        .Include(r => r.Playlist.Statistics)
        .Include(r => r.Playlist.Publisher);

    private void CreatePlaylistInternal(GamePlaylist createInfo)
    {
        DateTimeOffset now = this._time.Now;
        
        createInfo.CreationDate = now;
        createInfo.LastUpdateDate = now;

        this.WriteEnsuringStatistics(createInfo.Publisher, () =>
        {
            this.GamePlaylists.Add(createInfo);

            if (!createInfo.IsRoot)
            {
                createInfo.Publisher.Statistics!.PlaylistCount++;
            }
        });

        this.GamePlaylistStatistics.Add(createInfo.Statistics = new GamePlaylistStatistics
        {
            PlaylistId = createInfo.PlaylistId,
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
        => this.GamePlaylistsIncluded.FirstOrDefault(p => p.PlaylistId == playlistId);
    
    public GamePlaylist? GetUserRootPlaylist(GameUser user)
        => this.GamePlaylistsIncluded.FirstOrDefault(p => p.IsRoot && p.PublisherId == user.UserId);

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
        this.WriteEnsuringStatistics(playlist.Publisher, () =>
        {
            // Remove all relations relating to this playlist
            this.LevelPlaylistRelations.RemoveRange(l => l.PlaylistId == playlist.PlaylistId);
            this.SubPlaylistRelations.RemoveRange(l => l.PlaylistId == playlist.PlaylistId || l.SubPlaylistId == playlist.PlaylistId);
            this.FavouritePlaylistRelations.RemoveRange(l => l.PlaylistId == playlist.PlaylistId);
            
            // Remove the playlist object
            this.GamePlaylists.Remove(playlist);

            // Only decrement the user's playlists count, decrementing all relation stats aswell might be way too much effort,
            // they will be recalculated in a while anyway
            playlist.Publisher.Statistics!.PlaylistCount--;
        });
    }

    public void AddPlaylistToPlaylist(GamePlaylist child, GamePlaylist parent)
    {
        // Make sure to not create a duplicate object
        if (this.SubPlaylistRelations.Any(p => p.SubPlaylistId == child.PlaylistId && p.PlaylistId == parent.PlaylistId))
            return;

        this.WriteEnsuringStatistics(parent, child, () =>
        {
            // Add the relation
            this.SubPlaylistRelations.Add(new SubPlaylistRelation
            {
                Playlist = parent,
                SubPlaylist = child,
                Timestamp = this._time.Now,
            });

            parent.Statistics!.SubPlaylistCount++;
            child.Statistics!.ParentPlaylistCount++;
        });
    }
    
    public void RemovePlaylistFromPlaylist(GamePlaylist child, GamePlaylist parent)
    {
        SubPlaylistRelation? relation =
            this.SubPlaylistRelations.FirstOrDefault(r => r.SubPlaylistId == child.PlaylistId && r.PlaylistId == parent.PlaylistId);

        if (relation == null)
            return;
            
        this.WriteEnsuringStatistics(parent, child, () =>
        {
            this.SubPlaylistRelations.Remove(relation);

            parent.Statistics!.SubPlaylistCount--;
            child.Statistics!.ParentPlaylistCount--;
        });
    }
    
    public void AddLevelToPlaylist(GameLevel level, GamePlaylist parent)
    {
        // Make sure to not create a duplicate object
        if (this.LevelPlaylistRelations.Any(p => p.LevelId == level.LevelId && p.PlaylistId == parent.PlaylistId))
            return;

        this.WriteEnsuringStatistics(level, parent, () =>
        {
            // Add the relation
            this.LevelPlaylistRelations.Add(new LevelPlaylistRelation
            {
                Level = level,
                Playlist = parent,
                // index of new relation = index of last relation + 1 = relation count (without new relation)
                Index = parent.Statistics!.LevelCount,
                Timestamp = this._time.Now,
            });

            parent.Statistics!.LevelCount++;
            level.Statistics!.ParentPlaylistCount++;
        });
    }
    
    public void RemoveLevelFromPlaylist(GameLevel level, GamePlaylist parent)
    {
        LevelPlaylistRelation? relation =
            this.LevelPlaylistRelations.FirstOrDefault(r => r.LevelId == level.LevelId && r.PlaylistId == parent.PlaylistId);

        if (relation == null)
            return;

        this.WriteEnsuringStatistics(level, parent, () =>
        {
            this.LevelPlaylistRelations.Remove(relation);

            parent.Statistics!.LevelCount--;
            level.Statistics!.ParentPlaylistCount--;
        });
    }

    public void ReorderLevelsInPlaylist(IEnumerable<int> levelIds, GamePlaylist parent)
    {
        IEnumerable<LevelPlaylistRelation> relations = this.LevelPlaylistRelations
            .Where(r => r.PlaylistId == parent.PlaylistId)
            .OrderBy(r => r.Index)
            .ToArray();
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

    // Levels in Playlist
    public DatabaseList<GameLevel> GetLevelsInPlaylist(GamePlaylist playlist, TokenGame game, int skip, int count)
        => new(this.LevelPlaylistRelationsIncluded
            .Where(r => r.PlaylistId == playlist.PlaylistId)
            .OrderBy(r => r.Index)
            .Select(l => l.Level)
            .FilterByGameVersion(game), skip, count);

    public int GetTotalLevelsInPlaylist(GamePlaylist playlist) 
        => this.LevelPlaylistRelations.Count(l => l.PlaylistId == playlist.PlaylistId);
    
    // Playlists containing Level
    private IQueryable<GamePlaylist> GetPlaylistsContainingLevelInternal(GameLevel level)
        => this.LevelPlaylistRelationsIncluded
            .Where(p => p.LevelId == level.LevelId)
            .OrderByDescending(r => r.Timestamp)
            .Select(r => r.Playlist);

    public DatabaseList<GamePlaylist> GetPlaylistsByAuthorContainingLevel(GameUser author, GameLevel level, int skip, int count)
        => new(GetPlaylistsContainingLevelInternal(level)
            .Where(p => p.PublisherId == author.UserId), skip, count);
    
    public DatabaseList<GamePlaylist> GetPlaylistsContainingLevel(GameLevel level, int skip, int count)
        => new(GetPlaylistsContainingLevelInternal(level), skip, count);
    
    public int GetTotalPlaylistsContainingLevel(GameLevel level) 
        => this.LevelPlaylistRelations.Count(l => l.LevelId == level.LevelId);
    
    // Playlists in Playlists
    public DatabaseList<GamePlaylist> GetPlaylistsInPlaylist(GamePlaylist playlist, int skip, int count)
        => new(this.SubPlaylistRelationsIncluded
            .Where(p => p.PlaylistId == playlist.PlaylistId)
            .OrderByDescending(r => r.Timestamp)
            .Select(l => l.SubPlaylist), skip, count);
    
    public int GetTotalPlaylistsInPlaylist(GamePlaylist playlist)
        => this.SubPlaylistRelations.Count(p => p.PlaylistId == playlist.PlaylistId);
    
    // Playlists containing Playlists
    internal IQueryable<GamePlaylist> GetPlaylistsContainingPlaylistInternal(GamePlaylist playlist)
        => this.SubPlaylistRelations
            .Where(p => p.SubPlaylistId == playlist.PlaylistId)
            .OrderByDescending(r => r.Timestamp)
            .Select(r => this.GamePlaylistsIncluded.First(p => p.PlaylistId == r.PlaylistId))
            .Where(p => !p.IsRoot);

    public DatabaseList<GamePlaylist> GetPlaylistsContainingPlaylist(GamePlaylist playlist, int skip, int count)
        => new(GetPlaylistsContainingPlaylistInternal(playlist), skip, count);
    
    public int GetTotalPlaylistsContainingPlaylist(GamePlaylist playlist)
        => this.SubPlaylistRelationsIncluded.Count(p => p.SubPlaylistId == playlist.PlaylistId && !p.Playlist.IsRoot);

    public DatabaseList<GamePlaylist> GetPlaylistsByAuthorContainingPlaylist(GameUser user, GamePlaylist playlist, int skip, int count)
        => new(GetPlaylistsContainingPlaylistInternal(playlist)
            .Where(p => p.PublisherId == user.UserId), skip, count);

    // Just Playlists
    public DatabaseList<GamePlaylist> GetPlaylistsByAuthor(GameUser author, int skip, int count)
        => new(this.GamePlaylistsIncluded
            .Where(p => p.PublisherId == author.UserId)
            .Where(p => !p.IsRoot)
            .OrderByDescending(p => p.LastUpdateDate), skip, count);
    
    public int GetTotalPlaylistsByAuthor(GameUser author)
        => this.GamePlaylists.Count(p => p.PublisherId == author.UserId && !p.IsRoot);
    
    public DatabaseList<GamePlaylist> GetNewestPlaylists(int skip, int count)
        => new(this.GamePlaylistsIncluded
            .Where(p => !p.IsRoot)
            .OrderByDescending(p => p.CreationDate), skip, count);

    public DatabaseList<GamePlaylist> GetMostHeartedPlaylists(int skip, int count)
        => new(this.GamePlaylistsIncluded
            .Where(p => p.Statistics!.FavouriteCount > 0 && !p.IsRoot)
            .OrderByDescending(p => p.Statistics!.FavouriteCount), skip, count);
    
    #region Favouriting Playlists

    public DatabaseList<GamePlaylist> GetPlaylistsFavouritedByUser(GameUser user, int skip, int count) 
        => new(this.FavouritePlaylistRelationsIncluded
            .Where(r => r.UserId == user.UserId)
            .OrderByDescending(r => r.Timestamp)
            .Select(r => r.Playlist), skip, count);
    
    public int GetTotalPlaylistsFavouritedByUser(GameUser user) 
        => this.FavouritePlaylistRelations.Count(r => r.UserId == user.UserId);

    public int GetTotalFavouritesForPlaylist(GamePlaylist playlist)
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
        this.WriteEnsuringStatistics(user, playlist, () => 
        {
            this.FavouritePlaylistRelations.Add(relation);

            user.Statistics!.FavouritePlaylistCount++;
            playlist.Statistics!.FavouriteCount++;
        });

        return true;
    }

    public bool UnfavouritePlaylist(GamePlaylist playlist, GameUser user)
    {
        FavouritePlaylistRelation? relation = this.FavouritePlaylistRelations
            .FirstOrDefault(r => r.PlaylistId == playlist.PlaylistId && r.UserId == user.UserId);

        if (relation == null) return false;

        this.WriteEnsuringStatistics(user, playlist, () => 
        {
            this.FavouritePlaylistRelations.Remove(relation);

            user.Statistics!.FavouritePlaylistCount--;
            playlist.Statistics!.FavouriteCount--;
        });

        return true;
    }

    #endregion
}