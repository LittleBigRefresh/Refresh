using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Users;

namespace Refresh.Core.Types.Categories.Users;

public class HeartedUsersByUserCategory : GameUserCategory
{
    public HeartedUsersByUserCategory() : base("hearted", [], false)
    {
        this.Name = "My Hearted Users";
        this.Description = "Users you've hearted.";
        this.FontAwesomeIcon = "heart";
        this.IconHash = "g820612";
    }

    public override DatabaseList<GameUser>? Fetch(RequestContext context, int skip, int count, DataContext dataContext, GameUser? user)
    {
        // Prefer username from query, but fallback to user passed into this category if it's missing
        string? username = context.QueryString["u"] ?? context.QueryString["username"];
        if (username != null) user = dataContext.Database.GetUserByUsername(username);

        if (user == null) return null;
        return dataContext.Database.GetUsersFavouritedByUser(user, skip, count);
    }
}