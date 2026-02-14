using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

public class CoolLevelsCategory : GameCategory
{
    public CoolLevelsCategory() : base("coolLevels", ["lbpcool", "lbp2cool", "cool", "hot"], false)
    {
        this.Name = "Cool Levels";
        this.Description = "Levels trending with players like you!";
        this.FontAwesomeIcon = "fire";
        this.IconHash = "g820625";
        this.PrimaryResultType = ResultType.Level;
    }

    public override DatabaseResultList? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _)
    {
        return new(dataContext.Database.GetCoolLevels(count, skip, dataContext.User, levelFilterSettings));
    }
}