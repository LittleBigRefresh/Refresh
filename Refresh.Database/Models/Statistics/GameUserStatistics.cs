using MongoDB.Bson;

namespace Refresh.Database.Models.Statistics;

public class GameUserStatistics
{
    [Required, Key] public ObjectId UserId { get; set; }
    public DateTimeOffset? RecalculateAt { get; set; } = null;
    public int Version { get; set; } = GameDatabaseContext.UserStatisticsVersion;
    
    public int FavouriteCount { get; set; }
    public int CommentCount { get; set; }
    public int LevelCount { get; set; }
    public int PhotosByUserCount { get; set; }
    public int PhotosWithUserCount { get; set; }
    public int ReviewCount { get; set; }
    public int FavouriteLevelCount { get; set; }
    public int FavouriteUserCount { get; set; }
    public int QueueCount { get; set; }
}