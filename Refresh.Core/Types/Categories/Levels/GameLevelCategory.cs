using System.Diagnostics.Contracts;
using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

[JsonObject(MemberSerialization.OptIn)]
public abstract class GameLevelCategory : GameCategory
{
    internal GameLevelCategory(string apiRoute, string gameRoute, bool requiresUser) : base(apiRoute, [gameRoute], requiresUser) {}
    
    internal GameLevelCategory(string apiRoute, string[] gameRoutes, bool requiresUser) : base(apiRoute, gameRoutes, requiresUser) {}

    [Pure]
    public abstract DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        ResultFilterSettings levelFilterSettings, GameUser? user);
}