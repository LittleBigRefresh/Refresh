using Bunkum.HttpServer;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

public class ByUserLevelCategory : LevelCategory
{
    internal ByUserLevelCategory() : base("byUser", "by", true, nameof(RealmDatabaseContext.GetLevelsByUser))
    {
        this.Name = "Levels by user";
        this.Description = "A list of levels by this user.";
        this.IconHash = "0";
    }

    public override IEnumerable<GameLevel>? Fetch(RequestContext context, RealmDatabaseContext database, GameUser? user)
    {
        user = database.GetUserByUsername(context.Request.QueryString["u"]);
        if (user == null) return null;
        
        return base.Fetch(context, database, user);
    }
}