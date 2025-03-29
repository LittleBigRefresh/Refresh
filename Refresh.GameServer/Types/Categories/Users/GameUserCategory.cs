using System.Diagnostics.Contracts;
using Bunkum.Core;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Categories.Users;

[JsonObject(MemberSerialization.OptIn)]
public abstract class GameUserCategory : GameCategory
{
    internal GameUserCategory(string apiRoute, string gameRoute, bool requiresUser) : base(apiRoute, [gameRoute], requiresUser) {}
    
    internal GameUserCategory(string apiRoute, string[] gameRoutes, bool requiresUser) : base(apiRoute, gameRoutes, requiresUser) {}

    [Pure]
    public abstract DatabaseList<GameUser>? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? user);
}