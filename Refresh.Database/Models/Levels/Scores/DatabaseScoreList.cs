using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Levels.Scores;

/// <summary>
/// An extension of DatabaseList (generic type ScoreWithRank) for game endpoints where the game also 
/// wants to know the requesting user's own score and rank alongside the returned scoreboard fragment.
/// </summary>
public class DatabaseScoreList : DatabaseList<ScoreWithRank>
{
    public DatabaseScoreList(IEnumerable<ScoreWithRank> items, int skip, int count, GameUser? requestingUser) : base(items, skip, count)
    {
        if (requestingUser != null)
        {
            this.OwnScore = items.FirstOrDefault(s => s.score.PlayerIds.Contains(requestingUser.UserId));
        }
    }

    public ScoreWithRank? OwnScore { get; set; }
}