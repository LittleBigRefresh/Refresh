using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

public class DeveloperLevelsCategory : GameCategory
{
    internal DeveloperLevelsCategory() : base("developer", [], false)
    {
        this.Name = "Story Levels";
        this.Description = "Levels from LittleBigPlanet's story mode.";
        this.FontAwesomeIcon = "certificate";
        this.IconHash = "g820604";
        this.Hidden = true; // TODO: Set to false when we import story level names
        this.PrimaryResultType = ResultType.Level;
    }

    public override DatabaseResultList? Fetch(RequestContext context, int skip, int count,
        DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => new(dataContext.Database.GetDeveloperLevels(count, skip, levelFilterSettings));
}