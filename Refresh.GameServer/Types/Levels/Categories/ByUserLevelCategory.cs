using Bunkum.HttpServer;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

public class ByUserLevelCategory : LevelCategory
{
    internal ByUserLevelCategory() : base("byUser", "by", true, nameof(RealmDatabaseContext.GetLevelsByUser))
    {
        // Technically this category can apply to any user, but since we fallback to the regular user this name & description still applies
        this.Name = "Levels by you";
        this.Description = "A list of levels created by you!";
        this.IconHash = "g820625";
        this.FontAwesomeIcon = "user";
    }

    public override IEnumerable<GameLevel>? Fetch(RequestContext context, int skip, int count, RealmDatabaseContext database, GameUser? user, object[]? extraArgs)
    {
        // Prefer username from query, but fallback to user passed into this category if it's missing
        string? username = context.QueryString["u"];
        if (username != null) user = database.GetUserByUsername(username);

        if (user == null) return null;
        
        return base.Fetch(context, skip, count, database, user, extraArgs);
    }
}