using Bunkum.Core;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

public class SearchLevelCategory : LevelCategory
{
    public const string SearchRoute = "search";
    
    internal SearchLevelCategory() : base(SearchRoute, "search", false)
    {
        this.Name = "Search";
        this.Description = "Search for new levels.";
        this.FontAwesomeIcon = "magnifying-glass";
        // no icon for now, too lazy to find
        this.Hidden = true; // The search category is not meant to be shown, as it requires a special implementation on all frontends
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count,
        MatchService matchService, IGameDatabaseContext database, GameUser? user, 
        LevelFilterSettings levelFilterSettings)
    {
        string? query = context.QueryString["query"]
                        ?? context.QueryString["textFilter"]; // LBP3 sends this instead of query
        if (query == null) return null;

        return database.SearchForLevels(count, skip, user, levelFilterSettings, query);
    }
}