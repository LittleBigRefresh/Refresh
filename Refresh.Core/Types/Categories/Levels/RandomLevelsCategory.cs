using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

public class RandomLevelsCategory : GameLevelCategory
{
    internal RandomLevelsCategory() : base("random", ["lbp2luckydip", "luckydip"], false)
    {
        this.Name = "Lucky Dip";
        this.Description = "A random assortment of levels!";
        this.FontAwesomeIcon = "shuffle";
        this.IconHash = "g820605";
    }
    
    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        ResultFilterSettings levelFilterSettings, GameUser? _) 
        => dataContext.Database.GetRandomLevels(count, skip, dataContext.User, levelFilterSettings);
}