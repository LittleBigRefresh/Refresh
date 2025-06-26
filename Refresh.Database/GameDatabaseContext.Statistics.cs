using System.Diagnostics;
using Refresh.Database.Models;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Statistics;

namespace Refresh.Database;

public partial class GameDatabaseContext // Statistics
{
    public RequestStatistics GetRequestStatistics()
    {
        RequestStatistics? statistics = this.RequestStatistics.FirstOrDefault();
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
    
    public IEnumerable<GameLevel> GetLevelsWithStatisticsNeedingUpdates()
    {
        DateTimeOffset now = this._time.Now;

        return this.GameLevels
            .Include(l => l.Statistics)
            .Where(l => l.Statistics != null)
            .Where(l => l.Statistics!.RecalculateAt <= now);
    }

    public bool EnsureLevelStatisticsCreated(GameLevel level)
    {
        if (level.Statistics != null) return false;

        level.Statistics = this.GameLevelStatistics.FirstOrDefault(s => s.LevelId == level.LevelId);

        if (level.Statistics != null) return false;

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
        this.RecalculateLevelRatingStatisticsInternal(level);

        level.Statistics.RecalculateAt = null;
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
    }

    private void MarkLevelStatisticsDirty(GameLevel level)
    {
        Debug.Assert(this.ChangeTracker.HasChanges(), "should be called in write (no changes detected)");
        Debug.Assert(level.Statistics != null);
        
        if(level.Statistics.RecalculateAt == null)
            level.Statistics.RecalculateAt = this._time.Now + TimeSpan.FromMinutes(1);
    }
}