using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

public class MostHeartedLevelsCategory : GameLevelCategory
{
    internal MostHeartedLevelsCategory() : base("mostHearted", "mostHearted", false)
    {
        this.Name = "Избранное сообщества";
        this.Description = "Самые любимые уровни сообщества за все время!";
        this.FontAwesomeIcon = "heart";
        this.IconHash = "g820607";
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count,
        DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => dataContext.Database.GetMostFavouritedLevels(count, skip, dataContext.User, levelFilterSettings);
}