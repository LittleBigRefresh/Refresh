// Uncomment to enable extra logging.
// Do not enable in production as this is very memory-heavy.
// #define COOL_DEBUG

#if !DEBUG
#undef COOL_DEBUG
#endif

using System.Diagnostics;
using NotEnoughLogs;
using Refresh.Database;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.Roles;

namespace Refresh.GameServer.Workers;

public class CoolLevelsWorker : IWorker
{
    public int WorkInterval => 600_000; // Every 10 minutes
    public void DoWork(DataContext context)
    {
        const int pageSize = 1000;
        DatabaseList<GameLevel> levels = context.Database.GetUserLevelsChunk(0, pageSize);
        
        // Don't do anything if there are no levels to process.
        if (levels.TotalItems <= 0) return;
        
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
                Log(context.Logger, LogLevel.Trace, "Calculating score for '{0}' ({1})", level.Title, level.LevelId);
                float decayMultiplier = CalculateLevelDecayMultiplier(context.Logger, now, level);
                
                // Calculate positive & negative score separately so we don't run into issues with
                // the multiplier having an opposite effect with the negative score as time passes
                float positiveScore = CalculatePositiveScore(level, context);
                float negativeScore = CalculateNegativeScore(level, context);
                
                // Increase to tweak how little negative score gets affected by decay
                const int negativeScoreDecayMultiplier = 2;
                
                // Weigh everything with the multiplier and set a final score
                float finalScore = (positiveScore * decayMultiplier) - (negativeScore * Math.Min(1.0f, decayMultiplier * negativeScoreDecayMultiplier));
                
                Log(context.Logger, LogLevel.Debug, "Score for '{0}' ({1}) is {2}", level.Title, level.LevelId, finalScore);
                scoresToSet.Add(level, finalScore);
                remaining--;
            }
            
            // Commit scores to database. This method lets us use a dictionary so we can batch everything in one write
            context.Database.SetLevelScores(scoresToSet);
            
            // Load the next page
            levels = context.Database.GetUserLevelsChunk(levels.NextPageIndex - 1, pageSize);
        }
        
        stopwatch.Stop();
        context.Logger.LogInfo(RefreshContext.CoolLevels,  "Calculated scores for {0} levels in {1}ms", levels.TotalItems, stopwatch.ElapsedMilliseconds);
    }
    
    [Conditional("COOL_DEBUG")]
    private static void Log(Logger logger, LogLevel level, ReadOnlySpan<char> format, params object[] args)
    {
        logger.Log(level, RefreshContext.CoolLevels, format, args);
    }

    private static float CalculateLevelDecayMultiplier(Logger logger, long now, GameLevel level)
    {
        const double secondsPerMonth = 30 * 24 * 3600;
        
        // Use months
        double publishDate = level.PublishDate.ToUnixTimeSeconds() / secondsPerMonth;
        double elapsedMonths = now / secondsPerMonth - publishDate;

        // Get a scale from 0.0f to 1.0f, the percent of decay, using an exponential decay function
        // https://www.desmos.com/calculator/87wbuh1gcy
        double multiplier = Math.Pow(Math.E, -elapsedMonths);
        
        Log(logger, LogLevel.Trace, "Decay multiplier is {0}", multiplier);
        return (float)multiplier;
    }

    private static float CalculatePositiveScore(GameLevel level, DataContext context)
    {
        // Start levels off with a few points to prevent one dislike from bombing the level
        // Don't apply this bonus to reuploads to discourage a flood of 15CR levels.
        float score = level.IsReUpload ? 0 : 15;
        
        const float positiveRatingPoints = 5;
        const float uniquePlayPoints = 0.1f;
        const float heartPoints = 10;
        const float trustedAuthorPoints = 5;

        if (level.TeamPicked)
            score += 50;
        
        int positiveRatings = context.Database.GetTotalRatingsForLevel(level, RatingType.Yay, false);
        int negativeRatings = context.Database.GetTotalRatingsForLevel(level, RatingType.Boo, false);
        int uniquePlays = context.Database.GetUniquePlaysForLevel(level, false);
        
        score += positiveRatings * positiveRatingPoints;
        score += uniquePlays * uniquePlayPoints;
        score += context.Database.GetFavouriteCountForLevel(level, false) * heartPoints;
        
        // Reward for a good ratio between plays and yays
        float ratingRatio = (positiveRatings - negativeRatings) / (float)uniquePlays;
        if (ratingRatio > 0.5f)
        {
            score += positiveRatings * (positiveRatingPoints * ratingRatio);
        }

        if (level.Publisher?.Role == GameUserRole.Trusted)
            score += trustedAuthorPoints;

        Log(context.Logger, LogLevel.Trace, "positiveScore is {0}", score);
        return score;
    }

    private static float CalculateNegativeScore(GameLevel level, DataContext context)
    {
        float penalty = 0;
        const float negativeRatingPenalty = 5;
        const float noAuthorPenalty = 10;
        const float restrictedAuthorPenalty = 50;
        const float bannedAuthorPenalty = 100;
        
        // The percentage of how much penalty should be applied at the end of the calculation.
        const float penaltyMultiplier = 0.75f;
        
        penalty += context.Database.GetTotalRatingsForLevel(level, RatingType.Boo, false) * negativeRatingPenalty;
        
        if (level.Publisher == null)
            penalty += noAuthorPenalty;
        else if (level.Publisher?.Role == GameUserRole.Restricted)
            penalty += restrictedAuthorPenalty;
        else if (level.Publisher?.Role == GameUserRole.Banned)
            penalty += bannedAuthorPenalty;

        Log(context.Logger, LogLevel.Trace, "negativeScore is {0}", penalty);
        return penalty * penaltyMultiplier;
    }
}