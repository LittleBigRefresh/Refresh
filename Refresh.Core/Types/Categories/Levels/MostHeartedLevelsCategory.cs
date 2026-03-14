using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

public class MostHeartedLevelsCategory : GameCategory
{
    internal MostHeartedLevelsCategory() : base("mostHearted", "mostHearted", false)
    {
        this.Name = "Избранное сообщества";
        this.Description = "Самые любимые уровни сообщества за все время!";
        this.FontAwesomeIcon = "heart";
        this.IconHash = "g820607";
        this.PrimaryResultType = ResultType.Level;
    }

    public override DatabaseResultList? Fetch(RequestContext context, int skip, int count,
        DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => new(dataContext.Database.GetMostFavouritedLevels(count, skip, dataContext.User, levelFilterSettings));
}