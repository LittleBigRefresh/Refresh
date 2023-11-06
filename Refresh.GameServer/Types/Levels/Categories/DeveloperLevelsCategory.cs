using Bunkum.Core;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

public class DeveloperLevelsCategory : LevelCategory
{
    internal DeveloperLevelsCategory() : base("developer", Array.Empty<string>(), false)
    {
        this.Name = "Story Levels";
        this.Description = "Levels from LittleBigPlanet's story mode.";
        this.FontAwesomeIcon = "certificate";
        this.IconHash = "g820604";
        this.Hidden = true; // TODO: Set to false when we import story level names
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count,
        MatchService matchService, GameDatabaseContext database, GameUser? user,
        LevelFilterSettings levelFilterSettings) 
        => database.GetDeveloperLevels(count, skip, levelFilterSettings);
}