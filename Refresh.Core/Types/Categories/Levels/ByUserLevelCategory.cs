using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

public class ByUserLevelCategory : GameCategory
{
    internal ByUserLevelCategory() : base("byUser", "by", true)
    {
        // Technically this category can apply to any user, but since we fallback to the regular user this name & description still applies
        this.Name = "My Published Levels";
        this.Description = "Levels you've shared with the community!";
        this.IconHash = "g820625";
        this.FontAwesomeIcon = "user";
        this.PrimaryResultType = ResultType.Level;
    }

    public override DatabaseResultList? Fetch(RequestContext context, int skip, int count,
        DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? user)
    {
        // Prefer username from query, but fallback to user passed into this category if it's missing
        string? username = context.QueryString["u"] ?? context.QueryString["username"];
        if (username != null) user = dataContext.Database.GetUserByUsername(username);

        if (user == null) return null;
        
        DatabaseList<GameLevel>? levels = dataContext.Database.GetLevelsByUser(user, count, skip, levelFilterSettings, dataContext.User);
        
        // If this is LBP1 (or anything similar), inject the user's own playlists as well, but only those from their root playlist.
        DatabaseList<GamePlaylist>? playlists = null;
        if (dataContext.Game is TokenGame.LittleBigPlanet1 or TokenGame.BetaBuild)
        {
            GamePlaylist? rootPlaylist = dataContext.Database.GetUserRootPlaylist(user);
            if (rootPlaylist != null)
            {
                playlists = dataContext.Database.GetPlaylistsInPlaylist(rootPlaylist, skip, count);
            }
        }
        
        return new(levels, null, playlists);
    }
}