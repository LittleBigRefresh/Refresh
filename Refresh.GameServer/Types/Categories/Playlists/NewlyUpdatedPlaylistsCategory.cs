using Bunkum.Core;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Playlists;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Categories.Playlists;

public class NewlyUpdatedPlaylistsCategory : GamePlaylistCategory
{
    internal NewlyUpdatedPlaylistsCategory() : base("newlyUpdated", [], false)
    {
        this.Name = "Newly Updated Playlists";
        this.Description = "Our most recently updated playlists!";
        this.IconHash = "g820615";
        this.FontAwesomeIcon = "calendar";
    }
    
    public override DatabaseList<GamePlaylist>? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => dataContext.Database.GetMostRecentlyUpdatedPlaylists(skip, count);
}