using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

public class RandomLevelsCategory : GameCategory
{
    internal RandomLevelsCategory() : base("random", ["lbp2luckydip", "luckydip"], false)
    {
        this.Name = "Lucky Dip";
        this.Description = "A random assortment of levels!";
        this.FontAwesomeIcon = "shuffle";
        this.IconHash = "g820605";
        this.PrimaryResultType = ResultType.Level;
    }
    
    public override DatabaseResultList? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => new(dataContext.Database.GetRandomLevels(count, skip, dataContext.User, levelFilterSettings));
}