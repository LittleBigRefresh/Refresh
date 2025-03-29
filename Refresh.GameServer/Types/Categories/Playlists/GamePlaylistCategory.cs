using System.Diagnostics.Contracts;
using Bunkum.Core;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Playlists;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Categories.Playlists;

[JsonObject(MemberSerialization.OptIn)]
public abstract class GamePlaylistCategory : GameCategory
{
    internal GamePlaylistCategory(string apiRoute, string gameRoute, bool requiresUser) : base(apiRoute, [gameRoute], requiresUser) {}
    
    internal GamePlaylistCategory(string apiRoute, string[] gameRoutes, bool requiresUser) : base(apiRoute, gameRoutes, requiresUser) {}

    [Pure]
    public abstract DatabaseList<GamePlaylist>? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? user);
}