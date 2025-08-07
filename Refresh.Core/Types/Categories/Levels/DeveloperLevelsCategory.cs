using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

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
        ResultFilterSettings levelFilterSettings, GameUser? _) 
        => dataContext.Database.GetDeveloperLevels(count, skip, levelFilterSettings);
}