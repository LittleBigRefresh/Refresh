using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Endpoints.Debugging;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using MongoDB.Bson;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Playlists;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game.Playlists;

public class PlaylistLbp3Endpoints : EndpointGroup 
{
    [GameEndpoint("playlists", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response CreatePlaylist(RequestContext context, DataContext dataContext, GameUser user, SerializedLbp3Playlist body)
    {
        dataContext.Logger.LogInfo(BunkumCategory.UserContent, $"body: '{body}'");
        
        // if the player has no root playlist yet, create a new one first
        if (user.RootPlaylist == null)
        {
            GamePlaylist rootPlaylist = GamePlaylist.ToGamePlaylist("My Playlists", null, user, true);
            dataContext.Database.CreatePlaylist(rootPlaylist);
            dataContext.Database.SetUserRootPlaylist(user, rootPlaylist);
        }

        // create the actual playlist and add it to the root playlist to have it show up in lbp1 too
        GamePlaylist playlist = dataContext.Database.CreatePlaylist(user, body, false);
        dataContext.Database.AddPlaylistToPlaylist(playlist, user.RootPlaylist!);

        return OK;
    }

    [GameEndpoint("playlists/{playlistId}", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response UpdatePlaylist(RequestContext context, DataContext dataContext, GameUser user, SerializedLbp3Playlist body, int playlistId)
    {
        dataContext.Logger.LogInfo(BunkumCategory.UserContent, $"Playlist Name: {body.Name}, Description: {body.Description}");

        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(playlistId);
        if (playlist == null) return NotFound;

        // Dont allow the wrong user to update playlists
        if (playlist.Publisher.UserId != user.UserId)
            return Unauthorized;
        
        dataContext.Database.UpdatePlaylist(playlist, body);
        
        return OK;
    }

    [GameEndpoint("playlists/{playlistId}/delete", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response DeletePlaylist(RequestContext context, DataContext dataContext, GameUser user, int playlistId)
    {
        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(playlistId);
        if (playlist == null) return NotFound;

        // Dont allow the wrong user to delete playlists
        if (playlist.Publisher.UserId != user.UserId)
            return Unauthorized;

        dataContext.Database.DeletePlaylist(playlist);
            
        return OK;
    }


    [GameEndpoint("playlists/{playlistId}/slots", HttpMethods.Get, ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedLevelList? GetPlaylistLevels(RequestContext context, DataContext dataContext, GameUser user, int playlistId)
    {
        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(playlistId);
        if (playlist == null) return null;

        (int skip, int count) = context.GetPageData();
        IEnumerable<GameLevel> levels = dataContext.Database.GetLevelsInPlaylist(playlist, dataContext.Game);

        return new SerializedLevelList
        {
            Items = GameLevelResponse.FromOldList(levels, dataContext).ToList(),
            Total = levels.Count(),
            NextPageStart = skip + 1
        };
    }

    [GameEndpoint("playlists/{playlistId}/slots", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response AddLevelsToPlaylist(RequestContext context, DataContext dataContext, SerializedLevelIdList body, GameUser user, int playlistId)
    {
        dataContext.Logger.LogInfo(BunkumCategory.UserContent, $"body: '{body.ToJson()}'");

        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(playlistId);
        if (playlist == null) return NotFound;

        // Dont let people add levels to other's playlists
        if (playlist.Publisher.UserId != user.UserId)
            return Unauthorized;

        foreach(int levelId in body.LevelIds)
        {
            GameLevel? level = dataContext.Database.GetLevelById(levelId);
            if (level != null) dataContext.Database.AddLevelToPlaylist(level, playlist);
            // if (level == null) dataContext.Logger.LogInfo(BunkumCategory.UserContent, $"Level ID '{levelId}' is null");
        }

        return OK;
    }

    [GameEndpoint("playlists/{playlistId}/order_slots", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response ReorderPlaylistLevels(RequestContext context, DataContext dataContext, GameUser user, int playlistId)
    {
        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(playlistId);
        if (playlist == null)
            return NotFound;

        // Dont let people reorder levels in other's playlists
        if (playlist.Publisher.UserId != user.UserId)
            return Unauthorized;

        // custom level order in playlists is not being tracked rn
        return NotImplemented;
    }

    [GameEndpoint("playlists/{playlistId}/slots/{levelId}/delete", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response RemoveLevelFromPlaylist(RequestContext context, DataContext dataContext, GameUser user, int playlistId, int levelId)
    {
        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(playlistId);
        if (playlist == null) return NotFound;

        GameLevel? level = dataContext.Database.GetLevelById(levelId);
        if (level == null) return NotFound;

        // Dont let people remove levels from other's playlists
        if (playlist.Publisher.UserId != user.UserId)
            return Unauthorized;

        dataContext.Database.RemoveLevelFromPlaylist(level, playlist);
        return OK;
    }

    [GameEndpoint("user/{username}/playlists", HttpMethods.Get, ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedLbp3PlaylistList? GetPlaylistsByUser(RequestContext context, DataContext dataContext, string username)
    {
        GameUser? user = dataContext.Database.GetUserByUsername(username);
        if (user == null) return null;

        (int skip, int count) = context.GetPageData();
        IEnumerable<GamePlaylist> playlists = dataContext.Database.GetPlaylistsByAuthor(user);

        return new SerializedLbp3PlaylistList 
        (
            SerializedLbp3Playlist.FromOldList(playlists.Skip(skip).Take(count), dataContext),
            playlists.Count(),
            skip
        );
    }

    [GameEndpoint("favouritePlaylists/{username}", HttpMethods.Get, ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedLbp3PlaylistList? GetFavouritedPlaylists(RequestContext context, DataContext dataContext, string username)
    {
        GameUser? user = dataContext.Database.GetUserByUsername(username);
        if (user == null) return null;

        (int skip, int count) = context.GetPageData();
        IEnumerable<GamePlaylist> playlists = dataContext.Database.GetPlaylistsFavouritedByUser(user);

        foreach(GamePlaylist playlist in playlists)
        {
            dataContext.Logger.LogInfo(BunkumCategory.UserContent, $"Playlist {playlist.Name} is in the hearted playlists list");
        }

        return new SerializedLbp3FavouritePlaylistList
        (
            SerializedLbp3Playlist.FromOldList(playlists.Skip(skip).Take(count), dataContext),
            playlists.Count(),
            skip
        );
    }

    [GameEndpoint("favourite/playlist/{playlistId}", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response FavouritePlaylist(RequestContext context, DataContext dataContext, GameUser user, int playlistId)
    {
        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(playlistId);
        if (playlist == null) return NotFound;

        dataContext.Database.FavouritePlaylist(playlist, user);
        return OK;
    }

    [GameEndpoint("unfavourite/playlist/{playlistId}", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response UnfavouritePlaylist(RequestContext context, DataContext dataContext, GameUser user, int playlistId)
    {
        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(playlistId);
        if (playlist == null) return NotFound;

        dataContext.Database.UnfavouritePlaylist(playlist, user);
        return OK;
    }
}