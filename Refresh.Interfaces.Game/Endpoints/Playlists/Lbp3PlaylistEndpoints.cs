using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.RateLimit;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Common.Constants;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Constants.Playlists;
using Refresh.Interfaces.Game.Types.Levels;
using Refresh.Interfaces.Game.Types.Lists;
using Refresh.Interfaces.Game.Types.Playlists;

namespace Refresh.Interfaces.Game.Endpoints.Playlists;

public class Lbp3PlaylistEndpoints : EndpointGroup 
{
    private const int UploadTimeoutDuration = PlaylistEndpointLimits.UploadTimeoutDuration;
    private const int MaxCreateAmount = PlaylistEndpointLimits.MaxCreateAmount;
    private const int MaxUpdateAmount = PlaylistEndpointLimits.MaxUpdateAmount;
    private const int UploadBlockDuration = PlaylistEndpointLimits.UploadBlockDuration;
    private const string CreateBucket = PlaylistEndpointLimits.CreateBucket;
    private const string UpdateBucket = PlaylistEndpointLimits.UpdateBucket;

    [GameEndpoint("playlists", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    [RateLimitSettings(UploadTimeoutDuration, MaxCreateAmount, UploadBlockDuration, CreateBucket)]
    public Response CreatePlaylist(RequestContext context, GameServerConfig config, DataContext dataContext, GameUser user, SerializedLbp3Playlist body)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;

        GamePlaylist? rootPlaylist = dataContext.Database.GetUserRootPlaylist(user);

        // if the player has no root playlist yet, create a new one first
        rootPlaylist ??= dataContext.Database.CreateRootPlaylist(user);

        // Trim name and description
        if (body.Name != null && body.Name.Length > UgcLimits.TitleLimit) 
            body.Name = body.Name[..UgcLimits.TitleLimit];

        if (body.Description != null && body.Description.Length > UgcLimits.DescriptionLimit)
            body.Description = body.Description[..UgcLimits.DescriptionLimit];

        // create the actual playlist and add it to the root playlist
        GamePlaylist playlist = dataContext.Database.CreatePlaylist(user, body);
        dataContext.Database.AddPlaylistToPlaylist(playlist, rootPlaylist!);

        // return the playlist we just created to have the game open to it immediately
        return new Response(SerializedLbp3Playlist.FromOld(playlist, dataContext), ContentType.Xml);
    }

    [GameEndpoint("playlists/{playlistId}", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    [RateLimitSettings(UploadTimeoutDuration, MaxUpdateAmount, UploadBlockDuration, UpdateBucket)]
    public Response UpdatePlaylist(RequestContext context, GameServerConfig config, DataContext dataContext, GameUser user, SerializedLbp3Playlist body, int playlistId)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;

        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(playlistId);
        if (playlist == null) 
            return NotFound;

        // don't allow the wrong user to update playlists
        if (playlist.PublisherId != user.UserId)
            return Unauthorized;
        
        // Trim name and description
        if (body.Name != null && body.Name.Length > UgcLimits.TitleLimit) 
            body.Name = body.Name[..UgcLimits.TitleLimit];

        if (body.Description != null && body.Description.Length > UgcLimits.DescriptionLimit)
            body.Description = body.Description[..UgcLimits.DescriptionLimit];
        
        dataContext.Database.UpdatePlaylist(playlist, body);
        
        // get playlist from database a second time to respond with it in its updated state
        // to have it immediately update in-game
        GamePlaylist newPlaylist = dataContext.Database.GetPlaylistById(playlistId)!;
        return new Response(SerializedLbp3Playlist.FromOld(newPlaylist, dataContext), ContentType.Xml);
    }

    [GameEndpoint("playlists/{playlistId}/delete", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response DeletePlaylist(RequestContext context, GameServerConfig config, DataContext dataContext, GameUser user, int playlistId)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;

        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(playlistId);
        if (playlist == null) 
            return NotFound;

        // don't allow the wrong user to delete playlists
        if (playlist.PublisherId != user.UserId)
            return Unauthorized;

        dataContext.Database.DeletePlaylist(playlist);
        return OK;
    }

    [GameEndpoint("playlists/{playlistId}/slots", HttpMethods.Get, ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedMinimalLevelList? GetPlaylistLevels(RequestContext context, DataContext dataContext, GameUser user, int playlistId)
    {
        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(playlistId);
        if (playlist == null)
            return null;

        DatabaseList<GameLevel> levels = dataContext.Database.GetLevelsInPlaylist(playlist, dataContext.Game, 0, 100);

        return new SerializedMinimalLevelList
        {
            Items = GameMinimalLevelResponse.FromOldList(levels.Items.ToArray(), dataContext).ToList(),
            Total = levels.TotalItems,
            NextPageStart = levels.NextPageIndex
        };
    }

    [GameEndpoint("playlists/{playlistId}/slots", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response AddLevelsToPlaylist(RequestContext context, GameServerConfig config, DataContext dataContext, SerializedLevelIdList body, GameUser user, int playlistId)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;

        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(playlistId);
        if (playlist == null) 
            return NotFound;

        // Dont let people add levels to other's playlists
        if (playlist.PublisherId != user.UserId) 
            return Unauthorized;

        foreach (int levelId in body.LevelIds)
        {
            GameLevel? level = dataContext.Database.GetLevelById(levelId);
            if (level == null) continue;

            dataContext.Database.AddLevelToPlaylist(level, playlist);
        }

        return OK;
    }

    [GameEndpoint("playlists/{playlistId}/order_slots", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response ReorderPlaylistLevels(RequestContext context, GameServerConfig config, DataContext dataContext, SerializedLevelIdList body, GameUser user, int playlistId)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;

        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(playlistId);
        if (playlist == null)
            return NotFound;

        // Dont let people reorder levels in other's playlists
        if (playlist.PublisherId != user.UserId)
            return Unauthorized;

        dataContext.Database.ReorderLevelsInPlaylist(body.LevelIds, playlist);
        return OK;
    }

    [GameEndpoint("playlists/{playlistId}/slots/{levelId}/delete", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response RemoveLevelFromPlaylist(RequestContext context, GameServerConfig config, DataContext dataContext, GameUser user, int playlistId, int levelId)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;

        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(playlistId);
        if (playlist == null) 
            return NotFound;

        GameLevel? level = dataContext.Database.GetLevelById(levelId);
        if (level == null) 
            return NotFound;

        // Dont let people remove levels from other's playlists
        if (playlist.PublisherId != user.UserId)
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
        if (user == null) 
            return null;

        DatabaseList<GamePlaylist> playlists = dataContext.Database.GetPlaylistsByAuthor(user, 0, 100);

        return new SerializedLbp3PlaylistList 
        {
            Items = SerializedLbp3Playlist.FromOldList(playlists.Items, dataContext).ToList()
        };
    }

    [GameEndpoint("favouritePlaylists/{username}", HttpMethods.Get, ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedLbp3PlaylistList? GetFavouritedPlaylists(RequestContext context, DataContext dataContext, string username)
    {
        GameUser? user = dataContext.Database.GetUserByUsername(username);
        if (user == null) 
            return null;

        (int skip, int count) = context.GetPageData();
        DatabaseList<GamePlaylist> playlists = dataContext.Database.GetPlaylistsFavouritedByUser(user, skip, count);

        return new SerializedLbp3FavouritePlaylistList
        {
            Items = SerializedLbp3Playlist.FromOldList(playlists.Items, dataContext).ToList()
        };
    }

    [GameEndpoint("favourite/playlist/{playlistId}", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response FavouritePlaylist(RequestContext context, GameServerConfig config, DataContext dataContext, GameUser user, int playlistId)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;

        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(playlistId);
        if (playlist == null) 
            return NotFound;

        dataContext.Database.FavouritePlaylist(playlist, user);
        return OK;
    }

    [GameEndpoint("unfavourite/playlist/{playlistId}", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response UnfavouritePlaylist(RequestContext context, GameServerConfig config, DataContext dataContext, GameUser user, int playlistId)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;

        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(playlistId);
        if (playlist == null) 
            return NotFound;

        dataContext.Database.UnfavouritePlaylist(playlist, user);
        return OK;
    }
}