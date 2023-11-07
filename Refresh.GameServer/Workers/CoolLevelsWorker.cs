using System.Diagnostics;
using Bunkum.Core.Storage;
using NotEnoughLogs;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Reviews;

namespace Refresh.GameServer.Workers;

public class CoolLevelsWorker : IWorker
{
    public int WorkInterval => 600_000; // Every 10 minutes
    public bool DoWork(Logger logger, IDataStore dataStore, GameDatabaseContext database)
    {
        DatabaseList<GameLevel> levels = database.GetAllUserLevels();

        long now = DateTimeOffset.Now.ToUnixTimeSeconds();

        Dictionary<GameLevel, float> scoresToSet = new();

        Stopwatch stopwatch = new();
        stopwatch.Start();

        foreach (GameLevel level in levels.Items)
        {
            logger.LogTrace(RefreshContext.CoolLevels, "Calculating score for '{0}' ({1})", level.Title, level.LevelId);
            float multiplier = CalculateLevelDecayMultiplier(logger, now, level);

            int positiveScore = CalculatePositiveScore(logger, level);
            int negativeScore = CalculateNegativeScore(logger, level);

            float finalScore = (positiveScore * multiplier) - (negativeScore * Math.Min(1.0f, multiplier * 2));
            logger.LogDebug(RefreshContext.CoolLevels, "Score for '{0}' ({1}) is {2}", level.Title, level.LevelId, finalScore);
            scoresToSet.Add(level, finalScore);
        }
        
        stopwatch.Stop();
        logger.LogInfo(RefreshContext.CoolLevels, "Calculated scores for {0} levels in {1}ms", levels.TotalItems, stopwatch.ElapsedMilliseconds);
        
        // commit scores to database
        database.SetLevelScores(scoresToSet);

        return false;
    }

    private static float CalculateLevelDecayMultiplier(Logger logger, long now, GameLevel level)
    {
        const int decayDays = 30 * 3;
        const int decaySeconds = decayDays * 24 * 3600;
        const float minimumMultiplier = 0.1f;
        
        long publishDate = level.PublishDate / 1000;
        long elapsed = now - publishDate;

        float multiplier = 1.0f - Math.Min(1.0f, (float)elapsed / decaySeconds);
        multiplier = Math.Max(minimumMultiplier, multiplier);
        
        logger.LogTrace(RefreshContext.CoolLevels, "Decay multiplier is {0}", multiplier);
        return multiplier;
    }

    private static int CalculatePositiveScore(Logger logger, GameLevel level)
    {
        int score = 0;
        const int positiveRatingPoints = 5;
        const int positiveReviewPoints = 5;
        const int uniquePlayPoints = 1;
        const int heartPoints = 5;

        if (level.TeamPicked) score += 10;
        
        score += level.Ratings.Count(r => r._RatingType == (int)RatingType.Yay) * positiveRatingPoints;
        score += level.UniquePlays.Count() * uniquePlayPoints;
        score += level.FavouriteRelations.Count() * heartPoints;

        logger.LogTrace(RefreshContext.CoolLevels, "Positive Score is {0}", score);
        return score;
    }

    private static int CalculateNegativeScore(Logger logger, GameLevel level)
    {
        int score = 0;
        const int negativeRatingPoints = 5;
        const int negativeReviewPoints = 5;
        
        score += level.Ratings.Count(r => r._RatingType == (int)RatingType.Boo) * negativeRatingPoints;

        logger.LogTrace(RefreshContext.CoolLevels, "Negative Score is {0}", score);
        return score;
    }
}