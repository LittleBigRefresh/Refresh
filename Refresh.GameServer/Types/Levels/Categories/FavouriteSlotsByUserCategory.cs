using Bunkum.Core;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Services;
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
    
    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count, MatchService matchService, IGameDatabaseContext database, GameUser? user,
        LevelFilterSettings levelFilterSettings)
    {
        if (user == null) return null;
        
        return database.GetLevelsFavouritedByUser(user, count, skip, levelFilterSettings);
    }
}