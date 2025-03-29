using Bunkum.Core;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Playlists;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Categories.Playlists;

public class ByUserPlaylistCategory : GamePlaylistCategory
{
    internal ByUserPlaylistCategory() : base("byUser", [], true)
    {
        this.Name = "Your Playlists";
        this.Description = "Your own playlists.";
        this.FontAwesomeIcon = "heart";
        this.IconHash = "g820611";
    }
    
    public override DatabaseList<GamePlaylist>? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? user)
    {
        // Prefer username from query, but fallback to user passed into this category if it's missing
        string? username = context.QueryString["u"] ?? context.QueryString["username"];
        if (username != null) user = dataContext.Database.GetUserByUsername(username);

        if (user == null) return null;
        
        return new DatabaseList<GamePlaylist>
        (
            dataContext.Database.GetPlaylistsByAuthor(user), 
            skip, 
            count
        );
    }
}