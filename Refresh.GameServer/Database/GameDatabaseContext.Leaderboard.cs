using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Leaderboard
{
    public bool SubmitScore(GameScore score, GameUser user, GameLevel level)
    {
        GameSubmittedScore newScore = new()
        {
            Score = score.Score,
            ScoreType = score.ScoreType,
            Level = level,
            Players = { user },
            ScoreSubmitted = DateTimeOffset.Now,
        };

        this._realm.Write(() =>
        {
            this._realm.Add(newScore);
        });

        return true;
    }
}