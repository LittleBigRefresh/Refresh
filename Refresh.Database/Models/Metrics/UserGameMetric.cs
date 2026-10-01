using MongoDB.Bson;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Metrics;

[PrimaryKey(nameof(UserId), nameof(Game), nameof(Platform))]
public class UserGameMetric
{
    [Required]
    public ObjectId UserId { get; set; }
    
    [Required, ForeignKey(nameof(UserId))]
    public GameUser User { get; set; } = null!;
    
    public TokenGame Game { get; set; }
    public TokenPlatform Platform { get; set; }
    
    public DateTimeOffset LastLoginAt { get; set; }
    public DateTimeOffset LastRoomUpdateAt { get; set; }
    
    // total time the user has spent on this game, regardless of slot type. Not just a sum of all UserSlotMetrics,
    // since those won't record time spent if it's below a configured threshold.
    public long TotalPlayTimeMinutes { get; set; } 
    public long TotalLogins { get; set; }
}