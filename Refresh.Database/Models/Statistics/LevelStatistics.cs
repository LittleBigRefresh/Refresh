namespace Refresh.Database.Models.Statistics;

public class GameLevelStatistics
{
    [Required, Key] public int LevelId { get; set; }

    public int FavouriteCount { get; set; }
    public int PlayCount { get; set; }
    public int UniquePlayCount { get; set; }
    public int CompletionCount { get; set; }
    public int ReviewCount { get; set; }
    public int CommentCount { get; set; }
    public int PhotoInLevelCount { get; set; }
    public int PhotoByPublisherCount { get; set; }
    public int YayCount { get; set; }
    public int BooCount { get; set; }
    public int NeutralCount { get; set; }
}