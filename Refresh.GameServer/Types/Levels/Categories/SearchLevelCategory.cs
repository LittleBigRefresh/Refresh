using Bunkum.HttpServer;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

public class SearchLevelCategory : LevelCategory
{
    internal SearchLevelCategory() : base("search", "search", false, nameof(RealmDatabaseContext.SearchForLevels))
    {
        // no name/description as this wont ever show up in-game
    }

    public override IEnumerable<GameLevel>? Fetch(RequestContext context, RealmDatabaseContext database, GameUser? user, object[]? extraArgs = null)
    {
        string? query = context.QueryString["query"];
        if (query == null) return null;

        extraArgs = new object[] { query };
        
        return base.Fetch(context, database, user, extraArgs);
    }
}