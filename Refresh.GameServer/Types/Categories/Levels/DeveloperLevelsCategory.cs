using Bunkum.Core;
using Refresh.Database.Query;
using Refresh.Database;
using Refresh.GameServer.Types.Data;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;

namespace Refresh.GameServer.Types.Categories.Levels;

public class DeveloperLevelsCategory : GameLevelCategory
{
    internal DeveloperLevelsCategory() : base("developer", [], false)
    {
        this.Name = "Story Levels";
        this.Description = "Levels from LittleBigPlanet's story mode.";
        this.FontAwesomeIcon = "certificate";
        this.IconHash = "g820604";
        this.Hidden = true; // TODO: Set to false when we import story level names
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count,
        DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => dataContext.Database.GetDeveloperLevels(count, skip, levelFilterSettings);
}