using Bunkum.Core;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

public class FavouriteSlotsByUserCategory : LevelCategory
{
    internal FavouriteSlotsByUserCategory() : base("hearted", "favouriteSlots", true)
    {
        this.Name = "My Favorites";
        this.Description = "Your personal list filled with your favourite levels!";
        this.FontAwesomeIcon = "heart";
        this.IconHash = "g820611";
    }
    
    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? user)
    {
        if (user == null) return null;
        
        return dataContext.Database.GetLevelsFavouritedByUser(user, count, skip, levelFilterSettings, dataContext.User);
    }
}