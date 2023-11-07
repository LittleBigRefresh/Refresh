using Bunkum.Core.Storage;
using NotEnoughLogs;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.Reviews;

namespace Refresh.GameServer.Workers;

public class CoolLevelsWorker : IWorker
{
    public int WorkInterval => 600_000; // Every 10 minutes
    public bool DoWork(Logger logger, IDataStore dataStore, GameDatabaseContext database)
    {
        DatabaseList<GameLevel> levels = database.GetAllUserLevels();

        long now = DateTimeOffset.Now.ToUnixTimeSeconds();

        foreach (GameLevel level in levels.Items)
        {
            logger.LogDebug(RefreshContext.CoolLevels, "Calculating score for '{0}' ({1})", level.Title, level.LevelId);
            float multiplier = CalculateLevelDecayMultiplier(logger, now, level);

            int positiveScore = CalculatePositiveScore(logger, level);
            int negativeScore = CalculateNegativeScore(logger, level);

            float finalScore = (positiveScore * multiplier) - negativeScore;
            logger.LogDebug(RefreshContext.CoolLevels, "Score for '{0}' ({1}) is {2}", level.Title, level.LevelId, finalScore);
        }

        return false;
    }

    private static float CalculateLevelDecayMultiplier(Logger logger, long now, GameLevel level)
    {
        const int decayDays = 90;
        const int decaySeconds = decayDays * 24 * 3600;
        const float minimumMultiplier = 0.1f;
        
        long publishDate = level.PublishDate / 1000;
        long elapsed = now - publishDate;

        float multiplier = 1.0f - Math.Min(1.0f, (float)elapsed / decaySeconds);
        multiplier = Math.Max(minimumMultiplier, multiplier);
        
        logger.LogDebug(RefreshContext.CoolLevels, "Decay multiplier is {0}", multiplier);
        return multiplier;
    }

    private static int CalculatePositiveScore(Logger logger, GameLevel level)
    {
        int score = 0;
        const int positiveRatingPoints = 1;
        const int positiveReviewPoints = 1;
        const int uniquePlayPoints = 1;
        const int heartPoints = 1;

        score += level.Ratings.Count(r => r._RatingType == (int)RatingType.Yay) * positiveRatingPoints;
        score += level.UniquePlays.Count() * uniquePlayPoints;
        score += level.FavouriteRelations.Count() * heartPoints;

        logger.LogDebug(RefreshContext.CoolLevels, "Positive Score is {0}", score);
        return score;
    }

    private static int CalculateNegativeScore(Logger logger, GameLevel level)
    {
        int score = 0;
        const int negativeRatingPoints = 1;
        const int negativeReviewPoints = 1;
        
        score += level.Ratings.Count(r => r._RatingType == (int)RatingType.Yay) * negativeRatingPoints;

        logger.LogDebug(RefreshContext.CoolLevels, "Negative Score is {0}", score);
        return -score;
    }
}