using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Users;

public class MostHeartedUsersCategory : GameCategory
{
    public MostHeartedUsersCategory() : base("mostHearted", [], false)
    {
        this.Name = "Top Creators";
        this.Description = "Our most hearted users!";
        this.FontAwesomeIcon = "heart";
        this.IconHash = "g820627";
        this.PrimaryResultType = ResultType.User;
    }

    public override DatabaseResultList? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _)
    {
        return new(dataContext.Database.GetMostFavouritedUsers(skip, count));
    }
}