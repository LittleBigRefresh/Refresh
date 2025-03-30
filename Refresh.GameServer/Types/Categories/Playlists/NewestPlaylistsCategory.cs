using Bunkum.Core;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Playlists;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Categories.Playlists;

public class NewestPlaylistsCategory : GamePlaylistCategory
{
    internal NewestPlaylistsCategory() : base("newest", [], false)
    {
        this.Name = "Newest Playlists";
        this.Description = "Our newest playlists, put together by people like you.";
        this.IconHash = "g820615";
        this.FontAwesomeIcon = "calendar";
    }
    
    public override DatabaseList<GamePlaylist>? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => dataContext.Database.GetNewestPlaylists(skip, count);
}