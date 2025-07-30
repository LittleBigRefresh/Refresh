namespace Refresh.Database.Models.Statistics;

public class GamePlaylistStatistics
{
    [Required, Key] public int PlaylistId { get; set; }
    public DateTimeOffset? RecalculateAt { get; set; } = null;
    public int Version { get; set; } = GameDatabaseContext.PlaylistStatisticsVersion;

    public int FavouriteCount { get; set; }
    public int ParentPlaylistCount { get; set; }
    public int LevelCount { get; set; }
    public int SubPlaylistCount { get; set; }
}