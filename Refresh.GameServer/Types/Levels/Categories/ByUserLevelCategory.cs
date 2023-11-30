using Bunkum.Core;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

public class ByUserLevelCategory : LevelCategory
{
    internal ByUserLevelCategory() : base("byUser", "by", true)
    {
        // Technically this category can apply to any user, but since we fallback to the regular user this name & description still applies
        this.Name = "My Published Levels";
        this.Description = "Levels you've shared with the community!";
        this.IconHash = "g820625";
        this.FontAwesomeIcon = "user";
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count,
        MatchService matchService, IGameDatabaseContext database, GameUser? user, 
        LevelFilterSettings levelFilterSettings)
    {
        // Prefer username from query, but fallback to user passed into this category if it's missing
        string? username = context.QueryString["u"];
        if (username != null) user = database.GetUserByUsername(username);

        if (user == null) return null;
        
        return database.GetLevelsByUser(user, count, skip, levelFilterSettings);
    }
}