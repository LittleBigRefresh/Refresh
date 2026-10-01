using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Metrics;
using Refresh.Database.Models.Users;

namespace Refresh.Database;

public partial class GameDatabaseContext // Metrics
{
    private IQueryable<UserGameMetric> UserGameMetricsIncluded => this.UserGameMetrics
        .Include(m => m.User);

    public UserGameMetric GetGameMetric(GameUser user, TokenGame game, TokenPlatform platform)
    {
        UserGameMetric? metric = this.UserGameMetricsIncluded
            .FirstOrDefault(m => m.UserId == user.UserId && m.Game == game && m.Platform == platform);
        
        // If it doesn't exist yet, create it
        if (metric == null)
        {
            metric = new()
            {
                UserId = user.UserId,
                User = user,
                Game = game,
                Platform = platform,
                LastSlotChangeAt = DateTimeOffset.MinValue,
                LastSlotType = GameSlotType.Pod,
                LastSlotId = 0,
                TotalPlayTimeMinutes = 0,
                TotalLogins = 0,
            };
            this.UserGameMetrics.Add(metric);
            this.Entry(user).State = EntityState.Unchanged; // avoid inserting user
            this.SaveChanges();
        }

        return metric;
    }
    
    public UserGameMetric UpdateLoginDateOnUserMetrics(GameUser user, TokenGame game, TokenPlatform platform)
    {
        UserGameMetric metric = this.GetGameMetric(user, game, platform);
        
        metric.LastLoginAt = this._time.Now;
        metric.TotalLogins++;
        this.Entry(user).State = EntityState.Unchanged; // avoid inserting user
        
        this.SaveChanges();
        return metric;
    }

    public long GetTotalPlayTimeByUser(GameUser user)
    {
        return this.UserGameMetrics
            .Where(m => m.UserId == user.UserId)
            .Sum(m => m.TotalPlayTimeMinutes);
    }
    
    // TODO should we also consider slot ID, or should we stick to just type for now?
    public UserGameMetric UpdatePlayTimeOnUserMetrics(GameUser user, TokenGame game, TokenPlatform platform)
    {
        UserGameMetric gameMetric = this.GetGameMetric(user, game, platform);
        DateTimeOffset now = this._time.Now;

        // Only actually increment if this is at least the second update request for this session.
        // We only want to track from the first /match onward, excluding time between login and first /match.
        // Technically we don't have to compare against MinValue since LastUpdateAt would also be MinValue in that case,
        // but probably doesn't hurt doing this explicitly as well.
        if (gameMetric.LastUpdateAt > gameMetric.LastLoginAt && gameMetric.LastUpdateAt > DateTimeOffset.MinValue)
        {
            long additionalMinutes = (now.ToUnixTimeSeconds() - gameMetric.LastUpdateAt.ToUnixTimeSeconds()) / 60;
            long minutesSinceSlotChange = (now.ToUnixTimeSeconds() - gameMetric.LastSlotChangeAt.ToUnixTimeSeconds()) / 60;
            
            // Always update play time for game, and only update play time for slot if above threshold.
            // If type has changed, update LastSlotChangeAt, idk whether we should change it if only ID changes though.
            if (minutesSinceSlotChange >= minThreshold)
            {
        
                // If we've also changed type, update the last metric's time instead of the current one
                if (currentSlotType != gameMetric.LastSlotType)
                {
                    UserSlotMetric lastSlotMetric = this.GetSlotMetric(user, gameMetric.LastSlotType, game, platform);
                    lastSlotMetric.TotalPlayTimeMinutes += additionalMinutes;
                    lastSlotMetric.LastUpdateAt = now;
                    this.UserSlotMetrics.Update(lastSlotMetric);

                    currentSlotMetric.LastEnteredAt = now;
                }
                else
                {
                    currentSlotMetric.TotalPlayTimeMinutes += additionalMinutes;
                }
                
                currentSlotMetric.LastUpdateAt = now;
                this.UserSlotMetrics.Update(currentSlotMetric);
            }
        }

        if (gameMetric.LastSlotType != currentSlotType)
        {
            gameMetric.LastSlotChangeAt = now;
            gameMetric.LastSlotType = currentSlotType;
        }
        
        // Always update this, since we will use it to determine whether we should update other metrics the next time (see above).
        gameMetric.LastUpdateAt = now;
        
        this.UserGameMetrics.Update(gameMetric);
        this.Entry(user).State = EntityState.Unchanged; // avoid inserting user
        this.SaveChanges();
        return gameMetric;
    }
}