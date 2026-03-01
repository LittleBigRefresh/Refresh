using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.RateLimit;
using Bunkum.Protocols.Http;
using Refresh.Common.Constants;
using Refresh.Core.Services;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Playlists;
using static Refresh.Core.RateLimits.PlaylistEndpointLimits;

namespace Refresh.Interfaces.APIv3.Endpoints;

public class PlaylistApiEndpoints : EndpointGroup
{
    private ApiError? ValidatePlaylist(ApiPlaylistCreationRequest body, GuidCheckerService guidChecker, GameDatabaseContext database)
    {
        if (body.Icon != null)
        {
            if (body.Icon.IsBlankHash())
                body.Icon = "0";
            
            else if (body.Icon!.StartsWith('g') && body.Icon.Length > 1)
            {
                bool isGuid = long.TryParse(body.Icon[1..], out long guid);
                if (!isGuid || (isGuid && !guidChecker.IsTextureGuid(TokenGame.LittleBigPlanet1, guid)))
                    return ApiValidationError.InvalidTextureGuidError;
            }
            else
            {
                GameAsset? icon = database.GetAssetFromHash(body.Icon);

                if (icon == null) 
                    return ApiValidationError.IconMissingError;

                if (icon.AssetType is not GameAssetType.Jpeg and not GameAssetType.Png)
                    return ApiValidationError.IconMustBeImageError;
            }
        }

        // Trim name and description
        if (body.Name != null && body.Name.Length > UgcLimits.TitleLimit) 
            body.Name = body.Name[..UgcLimits.TitleLimit];

        if (body.Description != null && body.Description.Length > UgcLimits.DescriptionLimit)
            body.Description = body.Description[..UgcLimits.DescriptionLimit];

        // Ensure the coordinates are in a valid range
        if (body.Location != null)
        {
            body.Location.X = Math.Clamp(body.Location.X, 0, ushort.MaxValue);
            body.Location.Y = Math.Clamp(body.Location.Y, 0, ushort.MaxValue);
        }

        return null;
    }

    [ApiV3Endpoint("playlists", HttpMethods.Post)]
    [DocSummary("Posts a new playlist")]
    [DocError(typeof(ApiValidationError), ApiValidationError.InvalidPlaylistIdWhen)]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.ParentPlaylistMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.NoPlaylistEditPermissionErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.IconMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.IconMustBeImageErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.InvalidTextureGuidErrorWhen)]
    [RateLimitSettings(UploadTimeoutDuration, MaxCreateAmount, UploadBlockDuration, CreateBucket)]
    [DocQueryParam("parentId", "If set, the new playlist will be added to the playlist specified by ID here instead of the root playlist. "
        + "If the specified playlist doesn't exist or is not owned by the user calling this endpoint, nothing will happen.")]
    public ApiResponse<ApiGamePlaylistResponse> CreatePlaylist(RequestContext context, DataContext dataContext,
        GameUser user, ApiPlaylistCreationRequest body)
    {
        ApiError? error = this.ValidatePlaylist(body, dataContext.GuidChecker, dataContext.Database);
        if (error != null) return error;

        string? parentIdStr = context.QueryString.Get("parentId");
        GamePlaylist? parent;

        if (parentIdStr != null)
        {
            bool parsed = int.TryParse(parentIdStr, out int parentId);
            if (!parsed) return ApiValidationError.InvalidPlaylistId;

            parent = dataContext.Database.GetPlaylistById(parentId);
            if (parent == null) return ApiNotFoundError.ParentPlaylistMissingError;

            if (parent.PublisherId != user.UserId) 
                return ApiValidationError.NoPlaylistEditPermissionError;
        }
        else
        {
            // Get the root playlist. If the user has none yet, create a new one.
            parent = dataContext.Database.GetUserRootPlaylist(user) 
                ?? dataContext.Database.CreateRootPlaylist(user);
        }

        GamePlaylist playlist = dataContext.Database.CreatePlaylist(user, body, false);
        dataContext.Database.AddPlaylistToPlaylist(playlist, parent);

        return ApiGamePlaylistResponse.FromOld(playlist, dataContext);
    }

    [ApiV3Endpoint("playlists/id/{id}", HttpMethods.Patch)]
    [DocSummary("Updates an existing playlist specified by ID")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.PlaylistMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.NoPlaylistEditPermissionErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.IconMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.IconMustBeImageErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.InvalidTextureGuidErrorWhen)]
    [RateLimitSettings(UploadTimeoutDuration, MaxUpdateAmount, UploadBlockDuration, UpdateBucket)]
    public ApiResponse<ApiGamePlaylistResponse> UpdatePlaylist(RequestContext context, DataContext dataContext,
        GameUser user, ApiPlaylistCreationRequest body, int id)
    {
        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(id);
        if (playlist == null) return ApiNotFoundError.PlaylistMissingError;

        if (user.UserId != playlist.PublisherId) 
            return ApiValidationError.NoPlaylistEditPermissionError;

        ApiError? error = this.ValidatePlaylist(body, dataContext.GuidChecker, dataContext.Database);
        if (error != null) return error;

        playlist = dataContext.Database.UpdatePlaylist(playlist, body);
        return ApiGamePlaylistResponse.FromOld(playlist, dataContext);
    }

    [ApiV3Endpoint("playlists/id/{id}", HttpMethods.Delete)]
    [DocSummary("Deletes an existing playlist specified by ID")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.PlaylistMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.NoPlaylistDeletePermissionErrorWhen)]
    public ApiOkResponse DeletePlaylist(RequestContext context, DataContext dataContext,
        GameUser user, int id)
    {
        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(id);
        if (playlist == null) return ApiNotFoundError.PlaylistMissingError;

        if (user.UserId != playlist.PublisherId) 
            return ApiValidationError.NoPlaylistDeletePermissionError;

        dataContext.Database.DeletePlaylist(playlist);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("playlists/id/{id}"), Authentication(false)]
    [DocSummary("Gets a playlist specified by ID")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.PlaylistMissingErrorWhen)]
    public ApiResponse<ApiGamePlaylistResponse> GetPlaylistById(RequestContext context, DataContext dataContext, int id)
    {
        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(id);
        if (playlist == null) return ApiNotFoundError.PlaylistMissingError;

        return ApiGamePlaylistResponse.FromOld(playlist, dataContext);
    }

    [ApiV3Endpoint("playlists/id/{playlistId}/addLevel/id/{levelId}", HttpMethods.Post)]
    [DocSummary("Adds a level by ID to a playlist by ID")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.ParentPlaylistMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.NoPlaylistEditPermissionErrorWhen)]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse AddLevelToPlaylist(RequestContext context, DataContext dataContext,
        GameUser user, int playlistId, int levelId)
    {
        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(playlistId);
        if (playlist == null) return ApiNotFoundError.ParentPlaylistMissingError;

        if (user.UserId != playlist.PublisherId) 
            return ApiValidationError.NoPlaylistEditPermissionError;

        GameLevel? level = dataContext.Database.GetLevelById(levelId);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        dataContext.Database.AddLevelToPlaylist(level, playlist);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("playlists/id/{playlistId}/removeLevel/id/{levelId}", HttpMethods.Post)]
    [DocSummary("Removes a level by ID from a playlist by ID")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.ParentPlaylistMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.NoPlaylistEditPermissionErrorWhen)]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse RemoveLevelFromPlaylist(RequestContext context, DataContext dataContext,
        GameUser user, int playlistId, int levelId)
    {
        GamePlaylist? playlist = dataContext.Database.GetPlaylistById(playlistId);
        if (playlist == null) return ApiNotFoundError.ParentPlaylistMissingError;

        if (user.UserId != playlist.PublisherId) 
            return ApiValidationError.NoPlaylistEditPermissionError;

        GameLevel? level = dataContext.Database.GetLevelById(levelId);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        dataContext.Database.RemoveLevelFromPlaylist(level, playlist);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("playlists/id/{playlistId}/addPlaylist/id/{subPlaylistId}", HttpMethods.Post)]
    [DocSummary("Adds a playlist by ID to another playlist by ID")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.ParentPlaylistMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.NoPlaylistEditPermissionErrorWhen)]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.SubPlaylistMissingErrorWhen)]
    public ApiOkResponse AddPlaylistToPlaylist(RequestContext context, DataContext dataContext,
        GameUser user, int playlistId, int subPlaylistId)
    {
        GamePlaylist? parent = dataContext.Database.GetPlaylistById(playlistId);
        if (parent == null) return ApiNotFoundError.ParentPlaylistMissingError;

        if (user.UserId != parent.PublisherId) 
            return ApiValidationError.NoPlaylistEditPermissionError;

        GamePlaylist? child = dataContext.Database.GetPlaylistById(subPlaylistId);
        if (child == null) return ApiNotFoundError.SubPlaylistMissingError;

        dataContext.Database.AddPlaylistToPlaylist(child, parent);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("playlists/id/{playlistId}/removePlaylist/id/{subPlaylistId}", HttpMethods.Post)]
    [DocSummary("Removes a playlist by ID from a playlist by ID")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.ParentPlaylistMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.NoPlaylistEditPermissionErrorWhen)]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.SubPlaylistMissingErrorWhen)]
    public ApiOkResponse RemovePlaylistFromPlaylist(RequestContext context, DataContext dataContext,
        GameUser user, int playlistId, int subPlaylistId)
    {
        GamePlaylist? parent = dataContext.Database.GetPlaylistById(playlistId);
        if (parent == null) return ApiNotFoundError.ParentPlaylistMissingError;

        if (user.UserId != parent.PublisherId) 
            return ApiValidationError.NoPlaylistEditPermissionError;

        GamePlaylist? child = dataContext.Database.GetPlaylistById(subPlaylistId);
        if (child == null) return ApiNotFoundError.SubPlaylistMissingError;

        dataContext.Database.RemovePlaylistFromPlaylist(child, parent);
        return new ApiOkResponse();
    }
}