using Bunkum.Core;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

public class SearchLevelCategory : LevelCategory
{
    internal SearchLevelCategory() : base("search", "search", false, nameof(GameDatabaseContext.SearchForLevels))
    {
        this.Name = "Search";
        this.Description = "Search for new levels.";
        this.FontAwesomeIcon = "magnifying-glass";
        // no icon for now, too lazy to find
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count,
        MatchService matchService, GameDatabaseContext database, GameUser? user, TokenGame gameVersion,
        object[]? extraArgs)
    {
        string? query = context.QueryString["query"];
        if (query == null) return null;

        extraArgs = new object[] { query };
        
        return base.Fetch(context, skip, count, matchService, database, user, gameVersion, extraArgs);
    }
}