using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Users;

public class SearchUserByEmailCategory : GameCategory
{
    public SearchUserByEmailCategory() : base("searchAddress", [], true)
    {
        this.Name = "Search by address";
        this.Description = "Search users by (incomplete) email addresses or domains. This category is staff-only.";
        this.FontAwesomeIcon = "magnifying-glass";
        this.Hidden = true; // The search category is not meant to be shown, as it requires a special implementation on all frontends
        this.PrimaryResultType = ResultType.User;
    }

    public override DatabaseResultList? Fetch(RequestContext context, int skip, int count, DataContext dataContext, 
        LevelFilterSettings levelFilterSettings, GameUser? user)
    {
        // This category already isn't accessible in-game and by non-staff users (and it must stay that way),
        // but let's safeguard it regardless. This category in particular would be dangerous if it somehow
        // got exposed to non-staff users in the future anyway, even if the endpoint exposing it didn't return extended info.
        if (user == null)
        {
            context.Logger.LogWarning(BunkumCategory.LevelCategories, 
                $"Unauthed user attempted to search users by email address! " +
                $"We blocked this request, but it shouldn't be possible for them to reach this category to begin with. Please report this.");
            return null;
        }
        if (user.Role < GameUserRole.Moderator)
        {
            context.Logger.LogWarning(BunkumCategory.LevelCategories, 
                $"Non-staff user {user} attempted to search users by email address! " +
                $"We blocked this request, but it shouldn't be possible for them to reach this category to begin with. Please report this.");
            return null;
        }
        if (!context.IsApi())
        {
            context.Logger.LogWarning(BunkumCategory.LevelCategories, 
                $"User {user} attempted to search users by email address from non-API client {dataContext.Game}! " +
                $"We blocked this request, but it shouldn't be possible for them to reach this category to begin with. Please report this.");
            return null;
        }

        string? query = context.QueryString["query"];
        if (query == null) return null;
        
        return new(dataContext.Database.SearchForUsersByEmailAddress(count, skip, query));
    }
}