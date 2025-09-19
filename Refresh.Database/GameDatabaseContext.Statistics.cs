using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Refresh.Common.Constants;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Statistics;
using Refresh.Database.Models.Users;

namespace Refresh.Database;

public partial class GameDatabaseContext // Statistics
{
    public RequestStatistics GetRequestStatistics()
    {
        RequestStatistics? statistics = this.RequestStatistics
            .OrderBy(l => l.Id)
            .FirstOrDefault();
        if (statistics != null) return statistics;

        statistics = new RequestStatistics();
        this.Write(() =>
        {
            this.RequestStatistics.Add(statistics);
        });

        return statistics;
    }
    
    public void IncrementRequests(int api, int game)
    {
        RequestStatistics statistics = this.GetRequestStatistics();
        this.Write(() =>
        {
            statistics.ApiRequests += api;
            statistics.GameRequests += game;
        });
    }
    
    private void WriteEnsuringStatistics(GameUser user, GameLevel level, Action action)
    {
        this.Write(() =>
        {
            this.CalculateUserStatisticsIfNotPresent(user);
            this.CalculateLevelStatisticsIfNotPresent(level);
            action();
            this.MarkUserStatisticsDirty(user);
            this.MarkLevelStatisticsDirty(level);
        });
    }

    private void WriteEnsuringStatistics(GameUser user, GamePlaylist playlist, Action action)
    {
        this.Write(() =>
        {
            this.CalculateUserStatisticsIfNotPresent(user);
            this.CalculatePlaylistStatisticsIfNotPresent(playlist);
            action();
            this.MarkUserStatisticsDirty(user);
            this.MarkPlaylistStatisticsDirty(playlist);
        });
    }

    private void WriteEnsuringStatistics(GameLevel level, GamePlaylist playlist, Action action)
    {
        this.Write(() =>
        {
            this.CalculatePlaylistStatisticsIfNotPresent(playlist);
            this.CalculateLevelStatisticsIfNotPresent(level);
            action();
            this.MarkPlaylistStatisticsDirty(playlist);
            this.MarkLevelStatisticsDirty(level);
        });
    }

    private void WriteEnsuringStatistics(GamePlaylist parent, GamePlaylist child, Action action)
    {
        this.Write(() =>
        {
            this.CalculatePlaylistStatisticsIfNotPresent(parent);
            this.CalculatePlaylistStatisticsIfNotPresent(child);
            action();
            this.MarkPlaylistStatisticsDirty(parent);
            this.MarkPlaylistStatisticsDirty(child);
        });
    }
    
    #region Levels
    internal const int LevelStatisticsVersion = 4;
    
    public IEnumerable<GameLevel> GetLevelsWithStatisticsNeedingUpdates()
    {
        DateTimeOffset now = this._time.Now;

        return this.GameLevels
            .Include(l => l.Statistics)
            .Where(l => l.Statistics != null)
            .Where(l => l.Statistics!.RecalculateAt <= now || l.Statistics.Version != LevelStatisticsVersion);
    }

    public bool EnsureLevelStatisticsCreated(GameLevel level)
    {
        if (level.Statistics != null) return false;

        level.Statistics = this.GameLevelStatistics.FirstOrDefault(s => s.LevelId == level.LevelId);

        if (level.Statistics != null)
        {
#if DEBUG
            if(Debugger.IsAttached)
                Debugger.Break();
#endif

            return false;
        }

        level.Statistics = new GameLevelStatistics
        {
            LevelId = level.LevelId,
        };
        this.GameLevelStatistics.Add(level.Statistics);

        return true;
    }

    public void RecalculateLevelStatistics(GameLevel level)
    {
        this.EnsureLevelStatisticsCreated(level);
        this.Write(() =>
        {
            this.RecalculateLevelStatisticsInternal(level);
        });
    }
    
    public void CalculateLevelStatisticsIfNotPresent(GameLevel level)
    {
        if (!this.EnsureLevelStatisticsCreated(level)) return;

        this.Write(() =>
        {
            this.RecalculateLevelStatisticsInternal(level);
        });
    }

    private void WriteEnsuringStatistics(GameLevel level, Action action)
    {
        this.Write(() =>
        {
            this.CalculateLevelStatisticsIfNotPresent(level);
            action();
            this.MarkLevelStatisticsDirty(level);
        });
    }

    [SuppressMessage("ReSharper.DPA", "DPA0005: Database issues")]
    private void RecalculateLevelStatisticsInternal(GameLevel level)
    {
        Debug.Assert(level.Statistics != null);
        
        level.Statistics.FavouriteCount = this.GetTotalFavouritesForLevel(level);
        level.Statistics.FavouriteCountExcludingPublisher = this.GetTotalFavouritesForLevel(level, false);
        level.Statistics.PlayCount = this.GetTotalPlaysForLevel(level);
        level.Statistics.UniquePlayCount = this.GetTotalUniquePlaysForLevel(level);
        level.Statistics.UniquePlayCountExcludingPublisher = this.GetTotalUniquePlaysForLevel(level, false);
        level.Statistics.CompletionCount = this.GetTotalCompletionsForLevel(level);
        level.Statistics.ReviewCount = this.GetTotalReviewsForLevel(level);
        level.Statistics.CommentCount = this.GetTotalCommentsForLevel(level);
        level.Statistics.PhotoInLevelCount = this.GetTotalPhotosInLevel(level);
        level.Statistics.PhotoByPublisherCount = this.GetTotalPhotosInLevelByUser(level, level.Publisher);
        level.Statistics.ParentPlaylistCount = this.GetTotalPlaylistsContainingLevel(level);
        this.RecalculateLevelRatingStatisticsInternal(level);
        this.RecalculateLevelRecurringLabels(level);

        level.Statistics.RecalculateAt = null;
        level.Statistics.Version = LevelStatisticsVersion;
    }

    private void RecalculateLevelRatingStatisticsInternal(GameLevel level)
    {
        Debug.Assert(level.Statistics != null);
        
        level.Statistics.YayCount = this.GetTotalRatingsForLevel(level, RatingType.Yay);
        level.Statistics.YayCountExcludingPublisher = this.GetTotalRatingsForLevel(level, RatingType.Yay, false);
        level.Statistics.BooCount = this.GetTotalRatingsForLevel(level, RatingType.Boo);
        level.Statistics.BooCountExcludingPublisher = this.GetTotalRatingsForLevel(level, RatingType.Boo, false);
        level.Statistics.NeutralCount = this.GetTotalRatingsForLevel(level, RatingType.Neutral);
        level.Statistics.NeutralCountExcludingPublisher = this.GetTotalRatingsForLevel(level, RatingType.Neutral, false);
        level.Statistics.Karma = level.Statistics.YayCount - level.Statistics.BooCount;
    }

    private void RecalculateLevelRecurringLabels(GameLevel level)
    {
        Debug.Assert(level.Statistics != null);

        // Take the most recurring labels among the reviews for the level
        level.Statistics.RecurringLabels = this.GameReviews
            .Where(r => r.LevelId == level.LevelId)
            .SelectMany(r => r.Labels)
            .GroupBy(r => r)
            .Select(g => new { Label = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(UgcLimits.MaximumLabels)
            .Select(g => g.Label)
            .ToList();
    }

    private void MarkLevelStatisticsDirty(GameLevel level)
    {
        Debug.Assert(this.ChangeTracker.HasChanges(), "should be called in write (no changes detected)");
        Debug.Assert(level.Statistics != null);
        
        if(level.Statistics.RecalculateAt == null)
            level.Statistics.RecalculateAt = this._time.Now + TimeSpan.FromMinutes(5);
    }
    #endregion

    #region Users

    internal const int UserStatisticsVersion = 2;
    
    public IEnumerable<GameUser> GetUsersWithStatisticsNeedingUpdates()
    {
        DateTimeOffset now = this._time.Now;

        return this.GameUsers
            .Include(u => u.Statistics)
            .Where(u => u.Statistics != null)
            .Where(u => u.Statistics!.RecalculateAt <= now || u.Statistics.Version != UserStatisticsVersion);
    }

    public bool EnsureUserStatisticsCreated(GameUser user)
    {
        if (user.Statistics != null) return false;
        if (user.FakeUser) return true;

        user.Statistics = this.GameUserStatistics.FirstOrDefault(s => s.UserId == user.UserId);

        if (user.Statistics != null)
        {
#if DEBUG
            if(Debugger.IsAttached)
                Debugger.Break();
#endif

            return false;
        }

        user.Statistics = new GameUserStatistics
        {
            UserId = user.UserId,
        };
        this.GameUserStatistics.Add(user.Statistics);

        return true;
    }

    public void RecalculateUserStatistics(GameUser user)
    {
        this.EnsureUserStatisticsCreated(user);
        this.Write(() =>
        {
            this.RecalculateUserStatisticsInternal(user);
        });
    }
    
    public void CalculateUserStatisticsIfNotPresent(GameUser user)
    {
        if (!this.EnsureUserStatisticsCreated(user)) return;

        this.Write(() =>
        {
            this.RecalculateUserStatisticsInternal(user);
        });
    }

    private void WriteEnsuringStatistics(GameUser user, Action action)
    {
        this.Write(() =>
        {
            this.CalculateUserStatisticsIfNotPresent(user);
            action();
            this.MarkUserStatisticsDirty(user);
        });
    }

    [SuppressMessage("ReSharper.DPA", "DPA0005: Database issues")]
    private void RecalculateUserStatisticsInternal(GameUser user)
    {
        if (user.FakeUser)
        {
            user.Statistics = new GameUserStatistics
            {
                UserId = user.UserId,
            };
        }
        
        Debug.Assert(user.Statistics != null);
        
        user.Statistics.FavouriteCount = this.GetTotalUsersFavouritingUser(user);
        user.Statistics.CommentCount = this.GetTotalCommentsForProfile(user);
        user.Statistics.PhotosByUserCount = this.GetTotalPhotosByUser(user);
        user.Statistics.PhotosWithUserCount = this.GetTotalPhotosWithUser(user);
        user.Statistics.LevelCount = this.GetTotalLevelsByUser(user);
        user.Statistics.ReviewCount = this.GetTotalReviewsByUser(user);
        user.Statistics.QueueCount = this.GetTotalLevelsQueuedByUser(user);
        user.Statistics.FavouriteUserCount = this.GetTotalUsersFavouritedByUser(user);
        user.Statistics.FavouriteLevelCount = this.GetTotalLevelsFavouritedByUser(user);
        user.Statistics.FavouritePlaylistCount = this.GetTotalPlaylistsFavouritedByUser(user);
        user.Statistics.PlaylistCount = this.GetTotalPlaylistsByAuthor(user);

        user.Statistics.RecalculateAt = null;
        user.Statistics.Version = UserStatisticsVersion;
    }

    private void MarkUserStatisticsDirty(GameUser user)
    {
        Debug.Assert(this.ChangeTracker.HasChanges(), "should be called in write (no changes detected)");
        Debug.Assert(user.Statistics != null);
        
        if(user.Statistics.RecalculateAt == null)
            user.Statistics.RecalculateAt = this._time.Now + TimeSpan.FromMinutes(5);
    }

    #endregion

    #region Playlists

    internal const int PlaylistStatisticsVersion = 1;
    
    public IEnumerable<GamePlaylist> GetPlaylistsWithStatisticsNeedingUpdates()
    {
        DateTimeOffset now = this._time.Now;

        return this.GamePlaylists
            .Include(u => u.Statistics)
            .Where(u => u.Statistics != null)
            .Where(u => u.Statistics!.RecalculateAt <= now || u.Statistics.Version != PlaylistStatisticsVersion);
    }

    public bool EnsurePlaylistStatisticsCreated(GamePlaylist playlist)
    {
        if (playlist.Statistics != null) return false;

        playlist.Statistics = this.GamePlaylistStatistics.FirstOrDefault(s => s.PlaylistId == playlist.PlaylistId);

        if (playlist.Statistics != null)
        {
#if DEBUG
            if(Debugger.IsAttached)
                Debugger.Break();
#endif

            return false;
        }

        playlist.Statistics = new GamePlaylistStatistics
        {
            PlaylistId = playlist.PlaylistId,
        };
        this.GamePlaylistStatistics.Add(playlist.Statistics);

        return true;
    }

    public void RecalculatePlaylistStatistics(GamePlaylist playlist)
    {
        this.EnsurePlaylistStatisticsCreated(playlist);
        this.Write(() =>
        {
            this.RecalculatePlaylistStatisticsInternal(playlist);
        });
    }
    
    public void CalculatePlaylistStatisticsIfNotPresent(GamePlaylist playlist)
    {
        if (!this.EnsurePlaylistStatisticsCreated(playlist)) return;

        this.Write(() =>
        {
            this.RecalculatePlaylistStatisticsInternal(playlist);
        });
    }

    private void WriteEnsuringStatistics(GamePlaylist playlist, Action action)
    {
        this.Write(() =>
        {
            this.CalculatePlaylistStatisticsIfNotPresent(playlist);
            action();
            this.MarkPlaylistStatisticsDirty(playlist);
        });
    }

    private void RecalculatePlaylistStatisticsInternal(GamePlaylist playlist)
    {
        Debug.Assert(playlist.Statistics != null);
        
        playlist.Statistics.FavouriteCount = this.GetTotalFavouritesForPlaylist(playlist);
        playlist.Statistics.ParentPlaylistCount = this.GetTotalPlaylistsContainingPlaylist(playlist);
        playlist.Statistics.LevelCount = this.GetTotalLevelsInPlaylist(playlist);
        playlist.Statistics.SubPlaylistCount = this.GetTotalPlaylistsInPlaylist(playlist);

        playlist.Statistics.RecalculateAt = null;
        playlist.Statistics.Version = UserStatisticsVersion;
    }

    private void MarkPlaylistStatisticsDirty(GamePlaylist playlist)
    {
        Debug.Assert(this.ChangeTracker.HasChanges(), "should be called in write (no changes detected)");
        Debug.Assert(playlist.Statistics != null);
        
        if(playlist.Statistics.RecalculateAt == null)
            playlist.Statistics.RecalculateAt = this._time.Now + TimeSpan.FromMinutes(5);
    }

    #endregion
}