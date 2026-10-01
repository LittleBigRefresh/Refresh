using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Users;

public class SearchUserCategory : GameCategory
{
    internal SearchUserCategory() : base("search", [], false)
    {
        this.Name = "Search";
        this.Description = "Search for users by name and description.";
        this.FontAwesomeIcon = "magnifying-glass";
        // no icon for now, too lazy to find
        this.Hidden = true; // The search category is not meant to be shown, as it requires a special implementation on all frontends
        this.PrimaryResultType = ResultType.User;
    }

    public override DatabaseResultList? Fetch(RequestContext context, int skip, int count,
        DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _)
    {
        string? query = context.QueryString["query"]
                        ?? context.QueryString["textFilter"]; // LBP3 sends this instead of query
        if (query == null) return null;

        DatabaseList<GameUser> users = dataContext.Database.SearchForUsers(count, skip, query);
        return new(users);
    }
}