using Bunkum.Core;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

public class MostReplayedLevelsCategory : LevelCategory
{
    internal MostReplayedLevelsCategory() : base("mostReplayed", "mostPlays", false)
    {
        this.Name = "Replayable Levels";
        this.Description = "Levels people love to play over and over!";
        this.FontAwesomeIcon = "forward";
        this.IconHash = "g820608";
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count,
        MatchService matchService, GameDatabaseContext database, GameUser? accessor, 
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => database.GetMostReplayedLevels(count, skip, accessor, levelFilterSettings);
}