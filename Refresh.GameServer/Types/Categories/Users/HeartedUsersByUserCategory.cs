using Bunkum.Core;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Categories.Users;

public class HeartedUsersByUserCategory : GameUserCategory
{
    internal HeartedUsersByUserCategory() : base("hearted", [], true)
    {
        this.Name = "Your Favourite Users";
        this.Description = "Users you like.";
        this.FontAwesomeIcon = "heart";
        this.IconHash = "g820612";
    }
    
    public override DatabaseList<GameUser>? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? user)
    {
        // Prefer username from query, but fallback to user passed into this category if it's missing
        string? username = context.QueryString["u"] ?? context.QueryString["username"];
        if (username != null) user = dataContext.Database.GetUserByUsername(username);

        if (user == null) return null;
        
        return new DatabaseList<GameUser>
        (
            dataContext.Database.GetUsersFavouritedByUser(user, count, skip), 
            skip, 
            count
        );
    }
}