using Bunkum.Core;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Categories.Users;

public class MostHeartedUsersCategory : GameUserCategory
{
    internal MostHeartedUsersCategory() : base("mostHearted", [], false)
    {
        this.Name = "Most hearted Users";
        this.Description = "Our most beloved users!";
        this.IconHash = "g820608";
        this.FontAwesomeIcon = "calendar";
    }
    
    public override DatabaseList<GameUser>? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => dataContext.Database.GetMostHeartedUsers(skip, count);
}