using Bunkum.HttpServer;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

public class ByUserLevelCategory : LevelCategory
{
    internal ByUserLevelCategory() : base("byUser", "by", true, nameof(GameDatabaseContext.GetLevelsByUser))
    {
        // Technically this category can apply to any user, but since we fallback to the regular user this name & description still applies
        this.Name = "Levels by you";
        this.Description = "A list of levels created by you!";
        this.IconHash = "g820625";
        this.FontAwesomeIcon = "user";
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count,
        MatchService matchService, GameDatabaseContext database, GameUser? user, TokenGame gameVersion,
        object[]? extraArgs)
    {
        // Prefer username from query, but fallback to user passed into this category if it's missing
        string? username = context.QueryString["u"];
        if (username != null) user = database.GetUserByUsername(username);

        if (user == null) return null;
        
        return base.Fetch(context, skip, count, matchService, database, user, gameVersion, extraArgs);
    }
}