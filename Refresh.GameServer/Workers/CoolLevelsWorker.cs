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
        if (levels.TotalItems <= 0)
        {
            logger.LogWarning(RefreshContext.CoolLevels, "No levels to process for cool levels. If you're sure this server has levels, this is a bug.");
            return false;
        }

        long now = DateTimeOffset.Now.ToUnixTimeSeconds();
        // Create a dictionary so we can batch the write to the db
        Dictionary<GameLevel, float> scoresToSet = new(levels.TotalItems);

        Stopwatch stopwatch = new();
        stopwatch.Start();

        foreach (GameLevel level in levels.Items)
        {
            logger.LogTrace(RefreshContext.CoolLevels, "Calculating score for '{0}' ({1})", level.Title, level.LevelId);
            float decayMultiplier = CalculateLevelDecayMultiplier(logger, now, level);

            // Calculate positive & negative score separately so we don't run into issues with
            // the multiplier having an opposite effect with the negative score as time passes
            int positiveScore = CalculatePositiveScore(logger, level);
            int negativeScore = CalculateNegativeScore(logger, level);

            // Increase to tweak how little negative score gets affected by decay
            const int negativeScoreMultiplier = 2;
            
            // Weigh everything with the multiplier and set a final score
            float finalScore = (positiveScore * decayMultiplier) - (negativeScore * Math.Min(1.0f, decayMultiplier * negativeScoreMultiplier));
            
            logger.LogTrace(RefreshContext.CoolLevels, "Score for '{0}' ({1}) is {2}", level.Title, level.LevelId, finalScore);
            scoresToSet.Add(level, finalScore);
        }
        
        stopwatch.Stop();
        logger.LogInfo(RefreshContext.CoolLevels, "Calculated scores for {0} levels in {1}ms", levels.TotalItems, stopwatch.ElapsedMilliseconds);
        
        // Commit scores to database. This method lets us use a dictionary so we can batch everything in one write
        database.SetLevelScores(scoresToSet);
        
        return true; // Tell the worker manager we did work
    }

    private static float CalculateLevelDecayMultiplier(Logger logger, long now, GameLevel level)
    {
        const int decayMonths = 3;
        const int decaySeconds = decayMonths * 30 * 24 * 3600;
        const float minimumMultiplier = 0.1f;
        
        // Use seconds. Lets us not worry about float stuff
        long publishDate = level.PublishDate / 1000;
        long elapsed = now - publishDate;

        // Get a scale from 0.0f to 1.0f, the percent of decay
        float multiplier = 1.0f - Math.Min(1.0f, (float)elapsed / decaySeconds);
        multiplier = Math.Max(minimumMultiplier, multiplier); // Clamp to minimum multiplier
        
        logger.LogTrace(RefreshContext.CoolLevels, "Decay multiplier is {0}", multiplier);
        return multiplier;
    }

    private static int CalculatePositiveScore(Logger logger, GameLevel level)
    {
        int score = 0;
        const int positiveRatingPoints = 5;
        // const int positiveReviewPoints = 5;
        const int uniquePlayPoints = 1;
        const int heartPoints = 5;

        if (level.TeamPicked) score += 10;
        
        score += level.Ratings.Count(r => r._RatingType == (int)RatingType.Yay) * positiveRatingPoints;
        score += level.UniquePlays.Count() * uniquePlayPoints;
        score += level.FavouriteRelations.Count() * heartPoints;

        logger.LogTrace(RefreshContext.CoolLevels, "positiveScore is {0}", score);
        return score;
    }

    private static int CalculateNegativeScore(Logger logger, GameLevel level)
    {
        int score = 0;
        const int negativeRatingPoints = 5;
        // const int negativeReviewPoints = 5;
        
        score += level.Ratings.Count(r => r._RatingType == (int)RatingType.Boo) * negativeRatingPoints;

        logger.LogTrace(RefreshContext.CoolLevels, "negativeScore is {0}", score);
        return score;
    }
}