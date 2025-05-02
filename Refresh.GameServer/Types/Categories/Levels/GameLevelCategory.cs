using System.Diagnostics.Contracts;
using Bunkum.Core;
using Refresh.Database.Query;
using Refresh.Database;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Categories.Levels;

[JsonObject(MemberSerialization.OptIn)]
public abstract class GameLevelCategory : GameCategory
{
    internal GameLevelCategory(string apiRoute, string gameRoute, bool requiresUser) : base(apiRoute, [gameRoute], requiresUser) {}
    
    internal GameLevelCategory(string apiRoute, string[] gameRoutes, bool requiresUser) : base(apiRoute, gameRoutes, requiresUser) {}

    [Pure]
    public abstract DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? user);
}