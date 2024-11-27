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
        
        // if the player has no root playlist yet, create a new root playlist first before creating the actual playlist
        if (user.RootPlaylist == null)
        {
            // create a SerializedLbp3Playlist to reuse random location assignment from CreatePlaylist()
            SerializedLbp3Playlist RootPlaylistOld = new()
            {
                Name = "My Playlists"
            };

            GamePlaylist RootPlaylist = dataContext.Database.CreatePlaylist(user, RootPlaylistOld, true);
            dataContext.Database.SetUserRootPlaylist(user, RootPlaylist);
        }

        GamePlaylist playlist = dataContext.Database.CreatePlaylist(user, body, false);
        dataContext.Database.AddPlaylistToPlaylist(playlist, user.RootPlaylist!);

        return OK;
    }

    [GameEndpoint("playlists/{id}", HttpMethods.Post, ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [RequireEmailVerified]
    public SerializedLbp3Playlist? UpdatePlaylist(RequestContext context, DataContext dataContext, GameUser user, SerializedLbp3Playlist body, int id)
    {
        dataContext.Logger.LogInfo(BunkumCategory.UserContent, $"Playlist Name: {body.Name}, Description: {body.Description}");

        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(id);
        if (playlist == null) return null;

        Console.WriteLine("Playlist name: " + body.Name + ", description: " + body.Description);

        // Dont allow the wrong user to update playlists
        if (playlist.Publisher.UserId != user.UserId)
            return null;
        
        dataContext.Database.UpdatePlaylist(playlist, body);
        
        return body;
    }

    [GameEndpoint("playlists/{id}/delete", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response DeletePlaylist(RequestContext context, DataContext dataContext, GameUser user, int id)
    {
        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(id);
        if (playlist == null) return NotFound;

        // Dont allow the wrong user to delete playlists
        if (playlist.Publisher.UserId != user.UserId)
            return Unauthorized;

        dataContext.Database.DeletePlaylist(playlist);
            
        return OK;
    }


    [GameEndpoint("playlists/{id}/slots", HttpMethods.Get, ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedLevelList? GetPlaylistLevels(RequestContext context, DataContext dataContext, GameUser user, int id)
    {
        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(id);
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

    [GameEndpoint("playlists/{id}/slots", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response AddLevelsToPlaylist(RequestContext context, DataContext dataContext, SerializedLevelIdList body, int id)
    {
        dataContext.Logger.LogInfo(BunkumCategory.UserContent, $"body: '{body.ToJson()}'");

        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(id);
        if (playlist == null) return NotFound;

        foreach(int levelId in body.LevelIds)
        {
            GameLevel? level = dataContext.Database.GetLevelByIdAndType("user", levelId);
            if (level == null) dataContext.Logger.LogInfo(BunkumCategory.UserContent, $"Level ID '{levelId}' is null");
            if (level != null) dataContext.Database.AddLevelToPlaylist(level, playlist);
        }

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
            SerializedLbp3Playlist.FromOldList(playlists.Skip(skip).Take(count), dataContext).ToList(),
            playlists.Count(),
            skip + 1
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

        return new SerializedLbp3PlaylistList
        (
            SerializedLbp3Playlist.FromOldList(playlists.Skip(skip).Take(count), dataContext).ToList(),
            count,
            skip
        );
    }

    [GameEndpoint("favourite/playlist/{id}", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response FavouritePlaylist(RequestContext context, DataContext dataContext, GameUser user, string body, int id)
    {
        dataContext.Logger.LogInfo(BunkumCategory.UserContent, $"body: '{body}'");

        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(id);
        if (playlist == null) return NotFound;

        dataContext.Database.FavouritePlaylist(playlist, user);
        return OK;
    }

    [GameEndpoint("unfavourite/playlist/{id}", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response UnfavouritePlaylist(RequestContext context, DataContext dataContext, GameUser user, string body, int id)
    {
        dataContext.Logger.LogInfo(BunkumCategory.UserContent, $"body: '{body}'");

        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(id);
        if (playlist == null) return NotFound;

        dataContext.Database.UnfavouritePlaylist(playlist, user);
        return OK;
    }
}