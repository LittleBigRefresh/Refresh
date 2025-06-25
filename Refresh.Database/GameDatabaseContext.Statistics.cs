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

    public void RecalculateLevelStatistics(GameLevel level)
    {
        if (level.Statistics == null)
        {
            level.Statistics = this.GameLevelStatistics.FirstOrDefault(s => s.LevelId == level.LevelId);

            if (level.Statistics == null)
            {
                level.Statistics = new GameLevelStatistics
                {
                    LevelId = level.LevelId,
                };
                this.GameLevelStatistics.Add(level.Statistics);
            }
        }
        
        this.Write(() =>
        {
            level.Statistics.FavouriteCount = this.GetTotalFavouritesForLevel(level);
            level.Statistics.PlayCount = this.GetTotalPlaysForLevel(level);
            level.Statistics.UniquePlayCount = this.GetTotalUniquePlaysForLevel(level);
            level.Statistics.CompletionCount = this.GetTotalCompletionsForLevel(level);
            level.Statistics.ReviewCount = this.GetTotalReviewsForLevel(level);
            level.Statistics.CommentCount = this.GetTotalCommentsForLevel(level);
            level.Statistics.PhotoInLevelCount = this.GetTotalPhotosInLevel(level);
            level.Statistics.PhotoByPublisherCount = this.GetTotalPhotosInLevelByUser(level, level.Publisher);
            level.Statistics.YayCount = this.GetTotalRatingsForLevel(level, RatingType.Yay);
            level.Statistics.BooCount = this.GetTotalRatingsForLevel(level, RatingType.Boo);
            level.Statistics.NeutralCount = this.GetTotalRatingsForLevel(level, RatingType.Neutral);
        });
    }
}