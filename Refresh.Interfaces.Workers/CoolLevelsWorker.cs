// Uncomment to enable extra logging.
// Do not enable in production as this is very memory-heavy.
// #define COOL_DEBUG

#if !DEBUG
#undef COOL_DEBUG
#endif

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using NotEnoughLogs;
using Refresh.Core;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers;

public class CoolLevelsJob : WorkerJob
{
    public override int WorkInterval => 600_000; // Every 10 minutes

    [SuppressMessage("ReSharper.DPA", "DPA0005: Database issues")]
    public override void ExecuteJob(WorkContext context)
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
            
            foreach (GameLevel level in levels.Items.ToArray())
            {
                if(level.Statistics == null)
                    context.Database.RecalculateLevelStatistics(level);
                
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
            context.Database.SetLevelCoolRatings(scoresToSet);
            
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
        const double minimumMultiplier = 0.005f;
        
        // Use months
        double publishDate = level.PublishDate.ToUnixTimeSeconds() / secondsPerMonth;
        double elapsedMonths = now / secondsPerMonth - publishDate;

        // Get a scale from 0.0f to 1.0f, the percent of decay, using an exponential decay function
        // https://www.desmos.com/calculator/87wbuh1gcy
        double multiplier = Math.Pow(Math.E, -elapsedMonths);
        multiplier = Math.Max(minimumMultiplier, multiplier);
        
        Log(logger, LogLevel.Trace, "Decay multiplier is {0}", multiplier);
        return (float)multiplier;
    }

    private static float CalculatePositiveScore(GameLevel level, WorkContext context)
    {
        Debug.Assert(level.Statistics != null);

        // Start levels off with a few points to prevent one dislike from bombing the level
        // Don't apply this bonus to reuploads to discourage a flood of 15CR levels.
        float score = level.IsReUpload ? 0 : 15;
        
        const float uniquePlayPoints = 0.1f;
        const float trustedAuthorPoints = 5;
        
        float positiveRatingPoints = level.GameVersion switch
        {
            TokenGame.LittleBigPlanet1 => 2.5f,
            TokenGame.LittleBigPlanet2 => 7.5f,
            TokenGame.LittleBigPlanet3 => 30f, // yays are apparently hard to get to in LBP3; see example disparity with https://lbp.lbpbonsai.com/level/6961
            
            TokenGame.LittleBigPlanetVita => 15f,
            TokenGame.LittleBigPlanetPSP => 15f,
            _ => 5,
        };

        float heartPoints = level.GameVersion switch {
            TokenGame.LittleBigPlanet3 => 15f,
            
            TokenGame.LittleBigPlanetVita => 15f,
            TokenGame.LittleBigPlanetPSP => 15f,
            _ => 10,
        };

        if (level.TeamPicked)
            score += 100;
        
        int positiveRatings = level.Statistics.YayCountExcludingPublisher;
        int negativeRatings = level.Statistics.BooCountExcludingPublisher;
        int uniquePlays = level.Statistics.UniquePlayCountExcludingPublisher;
        
        score += positiveRatings * positiveRatingPoints;
        score += uniquePlays * uniquePlayPoints;
        score += level.Statistics.FavouriteCountExcludingPublisher * heartPoints;
        
        // Reward for a good ratio between plays and yays.
        // Doesn't apply to LBP1 levels.
        float ratingRatio = (positiveRatings - negativeRatings) / (float)uniquePlays;
        if (ratingRatio > 0.5f && level.GameVersion != TokenGame.LittleBigPlanet1)
        {
            score += positiveRatings * (positiveRatingPoints * ratingRatio);
        }

        if (level.Publisher?.Role == GameUserRole.Trusted)
            score += trustedAuthorPoints;

        if (level.IsReUpload)
            score *= 0.8f;

        Log(context.Logger, LogLevel.Trace, "positiveScore is {0}", score);
        return score;
    }

    private static float CalculateNegativeScore(GameLevel level, WorkContext context)
    {
        Debug.Assert(level.Statistics != null);

        float penalty = 0;
        const float negativeRatingPenalty = 5;
        const float noAuthorPenalty = 10;
        const float restrictedAuthorPenalty = 50;
        const float bannedAuthorPenalty = 100;
        
        // The percentage of how much penalty should be applied at the end of the calculation.
        const float penaltyMultiplier = 0.75f;
        
        penalty += level.Statistics.BooCountExcludingPublisher * negativeRatingPenalty;
        
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