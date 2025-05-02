using Bunkum.Core;
using Refresh.Database.Query;
using Refresh.Database;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Categories.Levels;

public class SearchLevelCategory : GameLevelCategory
{
    internal SearchLevelCategory() : base("search", "search", false)
    {
        this.Name = "Search";
        this.Description = "Search for new levels.";
        this.FontAwesomeIcon = "magnifying-glass";
        // no icon for now, too lazy to find
        this.Hidden = true; // The search category is not meant to be shown, as it requires a special implementation on all frontends
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count,
        DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _)
    {
        string? query = context.QueryString["query"]
                        ?? context.QueryString["textFilter"]; // LBP3 sends this instead of query
        if (query == null) return null;

        return dataContext.Database.SearchForLevels(count, skip, dataContext.User, levelFilterSettings, query);
    }
}