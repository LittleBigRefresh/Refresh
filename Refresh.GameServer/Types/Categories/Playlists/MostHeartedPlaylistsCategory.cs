using Bunkum.Core;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Playlists;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Categories.Playlists;

public class MostHeartedPlaylistsCategory : GamePlaylistCategory
{
    internal MostHeartedPlaylistsCategory() : base("mostHearted", [], false)
    {
        this.Name = "Most Hearted Playlists";
        this.Description = "Our most popular playlists of all-time!";
        this.FontAwesomeIcon = "heart";
        this.IconHash = "g820607";
    }
    
    public override DatabaseList<GamePlaylist>? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => dataContext.Database.GetMostHeartedPlaylists(skip, count);
}