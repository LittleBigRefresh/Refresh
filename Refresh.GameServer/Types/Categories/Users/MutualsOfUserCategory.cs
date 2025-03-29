using Bunkum.Core;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Categories.Users;

public class MutualsOfUserCategory : GameUserCategory
{
    internal MutualsOfUserCategory() : base("mutuals", [], true)
    {
        this.Name = "Your Mutuals";
        this.Description = "Your mutuals; people who have hearted you and who you have hearted.";
        this.FontAwesomeIcon = "heart";
        this.IconHash = "g820611";
    }
    
    public override DatabaseList<GameUser>? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? user)
    {
        if (user == null) return null;
        
        return new DatabaseList<GameUser>
        (
            dataContext.Database.GetUsersMutuals(user), 
            skip, 
            count
        );
    }
}