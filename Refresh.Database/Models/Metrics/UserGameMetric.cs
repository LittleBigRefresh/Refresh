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
    
    /// <summary>
    ///  Total time the user has spent on this game, determined by the times inbetween their room update requests.
    /// </summary>
    public long TotalPlayTimeMinutes { get; set; } 
}