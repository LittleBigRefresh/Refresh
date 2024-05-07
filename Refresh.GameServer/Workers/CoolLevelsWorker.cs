// Uncomment to enable extra logging.
// Do not enable in production as this is very memory-heavy.
// #define COOL_DEBUG

#if !DEBUG
#undef COOL_DEBUG
#endif

using System.Diagnostics;
using Bunkum.Core.Storage;
using NotEnoughLogs;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.Roles;

namespace Refresh.GameServer.Workers;

public class CoolLevelsWorker : IWorker
{
    public int WorkInterval => 600_000; // Every 10 minutes
    public bool DoWork(Logger logger, IDataStore dataStore, GameDatabaseContext database)
    {
        const int pageSize = 1000;
        DatabaseList<GameLevel> levels = database.GetUserLevelsChunk(0, pageSize);
        
        // Don't do anything if there are no levels to process.
        if (levels.TotalItems <= 0) return false;
        
        int remaining = levels.TotalItems;

        long now = DateTimeOffset.Now.ToUnixTimeSeconds();
        
        // Create a dictionary so we can batch the write to the db
        Dictionary<GameLevel, float> scoresToSet = new(pageSize);

        Stopwatch stopwatch = new();
        stopwatch.Start();
        
        while (remaining > 0)
        {
            scoresToSet.Clear(); // Re-use the same dictionary object.
            
            foreach (GameLevel level in levels.Items)
            {
                Log(logger, LogLevel.Trace, "Calculating score for '{0}' ({1})", level.Title, level.LevelId);
                float decayMultiplier = CalculateLevelDecayMultiplier(logger, now, level);
                
                // Calculate positive & negative score separately so we don't run into issues with
                // the multiplier having an opposite effect with the negative score as time passes
                int positiveScore = CalculatePositiveScore(logger, level);
                int negativeScore = CalculateNegativeScore(logger, level);
                
                // Increase to tweak how little negative score gets affected by decay
                const int negativeScoreMultiplier = 2;
                
                // Weigh everything with the multiplier and set a final score
                float finalScore = (positiveScore * decayMultiplier) - (negativeScore * Math.Min(1.0f, decayMultiplier * negativeScoreMultiplier));
                
                Log(logger, LogLevel.Debug, "Score for '{0}' ({1}) is {2}", level.Title, level.LevelId, finalScore);
                scoresToSet.Add(level, finalScore);
                remaining--;
            }
            
            // Commit scores to database. This method lets us use a dictionary so we can batch everything in one write
            database.SetLevelScores(scoresToSet);
            
            // Load the next page
            levels = database.GetUserLevelsChunk(levels.Items.Count(), pageSize);
        }
        
        stopwatch.Stop();
        logger.LogInfo(RefreshContext.CoolLevels,  "Calculated scores for {0} levels in {1}ms", levels.TotalItems, stopwatch.ElapsedMilliseconds);
        
        return true; // Tell the worker manager we did work
    }
    
    [Conditional("COOL_DEBUG")]
    private static void Log(Logger logger, LogLevel level, ReadOnlySpan<char> format, params object[] args)
    {
        logger.Log(level, RefreshContext.CoolLevels, format, args);
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
        
        Log(logger, LogLevel.Trace, "Decay multiplier is {0}", multiplier);
        return multiplier;
    }

    private static int CalculatePositiveScore(Logger logger, GameLevel level)
    {
        int score = 15; // Start levels off with a few points to prevent one dislike from bombing the level
        const int positiveRatingPoints = 5;
        const int uniquePlayPoints = 1;
        const int heartPoints = 5;
        const int trustedAuthorPoints = 5;

        if (level.TeamPicked)
            score += 10;
        
        score += level.Ratings.Count(r => r._RatingType == (int)RatingType.Yay) * positiveRatingPoints;
        score += level.UniquePlays.Count() * uniquePlayPoints;
        score += level.FavouriteRelations.Count() * heartPoints;

        if (level.Publisher?.Role == GameUserRole.Trusted)
            score += trustedAuthorPoints;

        Log(logger, LogLevel.Trace, "positiveScore is {0}", score);
        return score;
    }

    private static int CalculateNegativeScore(Logger logger, GameLevel level)
    {
        int penalty = 0;
        const int negativeRatingPenalty = 5;
        const int noAuthorPenalty = 10;
        const int restrictedAuthorPenalty = 50;
        const int bannedAuthorPenalty = 100;
        
        penalty += level.Ratings.Count(r => r._RatingType == (int)RatingType.Boo) * negativeRatingPenalty;
        
        if (level.Publisher == null)
            penalty += noAuthorPenalty;
        else if (level.Publisher?.Role == GameUserRole.Restricted)
            penalty += restrictedAuthorPenalty;
        else if (level.Publisher?.Role == GameUserRole.Banned)
            penalty += bannedAuthorPenalty;

        Log(logger, LogLevel.Trace, "negativeScore is {0}", penalty);
        return penalty;
    }
}