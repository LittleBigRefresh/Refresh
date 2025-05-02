using Refresh.Database.Models.Authentication;

namespace Refresh.Database.Models.Levels.Scores;

public class MultiLeaderboard
{
    public readonly Dictionary<byte, DatabaseList<GameSubmittedScore>> Leaderboards;

    public MultiLeaderboard(GameDatabaseContext database, GameLevel level, TokenGame game)
    {
        this.Leaderboards = new Dictionary<byte, DatabaseList<GameSubmittedScore>>
        {
            //On all games set the 1 player leaderboards
            [1] = database.GetTopScoresForLevel(level, 10, 0, 1),
        };

        //On PSP, theres no multiplayer, so lets skip all the multiplayer/vs scoreboards
        if (game == TokenGame.LittleBigPlanetPSP) return;
        
        this.Leaderboards[2] = database.GetTopScoresForLevel(level, 10, 0, 2);
        this.Leaderboards[3] = database.GetTopScoresForLevel(level, 10, 0, 3);
        this.Leaderboards[4] = database.GetTopScoresForLevel(level, 10, 0, 4);
        this.Leaderboards[7] = database.GetTopScoresForLevel(level, 10, 0, 7);
    }
}