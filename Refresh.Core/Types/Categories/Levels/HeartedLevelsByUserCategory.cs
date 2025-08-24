using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

public class HeartedLevelsByUserCategory : GameLevelCategory
{
    internal HeartedLevelsByUserCategory() : base("hearted", "favouriteSlots", true)
    {
        this.Name = "My Favorites";
        this.Description = "Your personal list filled with your favourite levels!";
        this.FontAwesomeIcon = "heart";
        this.IconHash = "g820611";
    }
    
    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? user)
    {
        // Prefer username from query, but fallback to user passed into this category if it's missing
        string? username = context.QueryString["u"] ?? context.QueryString["username"];
        if (username != null) user = dataContext.Database.GetUserByUsername(username);

        if (user == null) return null;
        
        return dataContext.Database.GetLevelsFavouritedByUser(user, count, skip, levelFilterSettings, dataContext.User);
    }
}