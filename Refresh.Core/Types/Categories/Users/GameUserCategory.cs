using System.Diagnostics.Contracts;
using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Users;

namespace Refresh.Core.Types.Categories.Users;

[JsonObject(MemberSerialization.OptIn)]
public abstract class GameUserCategory : GameCategory
{
    internal GameUserCategory(string apiRoute, string gameRoute, bool requiresUser) : base(apiRoute, [gameRoute], requiresUser) {}
    
    internal GameUserCategory(string apiRoute, string[] gameRoutes, bool requiresUser) : base(apiRoute, gameRoutes, requiresUser) {}

    [Pure]
    public abstract DatabaseList<GameUser>? Fetch(RequestContext context, int skip, int count, DataContext dataContext, GameUser? user);
}