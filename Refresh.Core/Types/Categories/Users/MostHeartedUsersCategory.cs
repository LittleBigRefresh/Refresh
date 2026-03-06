using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Users;

namespace Refresh.Core.Types.Categories.Users;

public class MostHeartedUsersCategory : GameUserCategory
{
    public MostHeartedUsersCategory() : base("mostHearted", [], false)
    {
        this.Name = "Топовые пользователи";
        this.Description = "Больше всего лайков!";
        this.FontAwesomeIcon = "heart";
        this.IconHash = "g820627";
    }

    public override DatabaseList<GameUser>? Fetch(RequestContext context, int skip, int count, DataContext dataContext, GameUser? _)
    {
        return dataContext.Database.GetMostFavouritedUsers(skip, count);
    }
}