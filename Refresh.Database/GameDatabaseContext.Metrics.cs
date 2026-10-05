using Refresh.Common;
using Refresh.Database.Models.Authentication;
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
        
        // If it doesn't exist yet, create it. Timestamps will be properly set by their dedicated methods when needed.
        if (metric == null)
        {
            this._logger.LogDebug(RefreshContext.UserMetrics, $"Creating new game metric for {user} for game {game}/platform {platform}.");
            metric = new()
            {
                UserId = user.UserId,
                User = user,
                Game = game,
                Platform = platform,
                LastLoginAt = DateTimeOffset.MinValue,
                LastRoomUpdateAt = DateTimeOffset.MinValue,
                TotalPlayTimeMinutes = 0,
            };
            this.UserGameMetrics.Add(metric);

            this.TrackUserAsUnchanged(user);
            this.SaveChanges();
        }

        return metric;
    }

    public long GetTotalPlayTimeByUser(GameUser user)
    {
        return this.UserGameMetrics
            .Where(m => m.UserId == user.UserId)
            .Sum(m => m.TotalPlayTimeMinutes);
    }
    
    public UserGameMetric UpdateLoginDateOnUserMetrics(GameUser user, TokenGame game, TokenPlatform platform)
    {
        UserGameMetric metric = this.GetGameMetric(user, game, platform);
        DateTimeOffset now = this._time.Now;
        
        this._logger.LogDebug(RefreshContext.UserMetrics, $"Updating {user}'s last login date for game {game}/platform {platform}: from {metric.LastLoginAt} to {now}.");
        metric.LastLoginAt = now;
        
        this.TrackUserAsUnchanged(user);
        this.SaveChanges();
        return metric;
    }
    
    public UserGameMetric UpdatePlayTimeOnUserMetrics(GameUser user, TokenGame game, TokenPlatform platform)
    {
        UserGameMetric metric = this.GetGameMetric(user, game, platform);
        DateTimeOffset now = this._time.Now;
        this._logger.LogDebug(RefreshContext.UserMetrics, $"Checking whether to update {user}'s playtime for game {game}/platform {platform}: last login at {metric.LastLoginAt}, last room update at {metric.LastRoomUpdateAt}.");

        // Don't count the time between login and first /match, only count the time between /match requests,
        // which we would consider to be actual playtime. Also, explicitly don't increment time if this metric
        // hasn't been initialized yet. In that case LastLoginAt may also be at MinValue, but it might be better to just
        // check for it explicitly anyway.
        if (metric.LastRoomUpdateAt > metric.LastLoginAt && metric.LastRoomUpdateAt > DateTimeOffset.MinValue)
        {
            long additionalMinutes = (now.ToUnixTimeSeconds() - metric.LastRoomUpdateAt.ToUnixTimeSeconds()) / 60;
            this._logger.LogDebug(RefreshContext.UserMetrics, $"Updating {user}'s playtime for game {game}/platform {platform}: {metric.TotalPlayTimeMinutes} min + {additionalMinutes} min");
            metric.TotalPlayTimeMinutes += additionalMinutes;
        }
        
        // Always update this, since we will use it to determine whether we should update this metric the next time (see above).
        metric.LastRoomUpdateAt = now;
        
        this.UserGameMetrics.Update(metric);
        this.TrackUserAsUnchanged(user);
        this.SaveChanges();
        return metric;
    }
}