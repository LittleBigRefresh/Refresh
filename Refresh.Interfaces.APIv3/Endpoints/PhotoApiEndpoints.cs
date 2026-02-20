using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Bunkum.Protocols.Http;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Photos;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Documentation.Attributes;
using Refresh.Interfaces.APIv3.Documentation.Descriptions;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users.Photos;
using Refresh.Interfaces.APIv3.Extensions;

namespace Refresh.Interfaces.APIv3.Endpoints;

public class PhotoApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("photos/id/{id}", HttpMethods.Delete)]
    [DocSummary("Deletes an uploaded photo")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.PhotoMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.NoPhotoDeletionPermissionErrorWhen)]
    public ApiResponse<ApiEmptyResponse> DeletePhoto(RequestContext context, GameDatabaseContext database, GameUser user, int id)
    {
        GamePhoto? photo = database.GetPhotoById(id);
        if (photo == null) return ApiNotFoundError.PhotoMissingError;

        if (photo.Publisher.UserId != user.UserId)
            return ApiValidationError.NoPhotoDeletionPermissionError;

        database.RemovePhoto(photo);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("photos/by/{userIdType}/{id}"), Authentication(false)]
    [DocUsesPageData, DocSummary("Gets photos uploaded by a user specified by their username or UUID")]
    [DocError(typeof(ApiNotFoundError), "The user cannot be found")]
    public ApiListResponse<ApiGamePhotoResponse> PhotosByUser(RequestContext context, GameDatabaseContext database,
        [DocSummary(SharedParamDescriptions.UserIdParam)] string id, 
        [DocSummary(SharedParamDescriptions.UserIdTypeParam)] string userIdType, DataContext dataContext) 
    {
        GameUser? user = database.GetUserByIdAndType(userIdType, id);
        if (user == null) return ApiNotFoundError.Instance;

        (int skip, int count) = context.GetPageData();

        DatabaseList<GamePhoto> photos = database.GetPhotosByUser(user, count, skip);
        DatabaseList<ApiGamePhotoResponse> photosResponse = DatabaseListExtensions.FromOldList<ApiGamePhotoResponse, GamePhoto>(photos, dataContext);
        return photosResponse;
    }
    
    [ApiV3Endpoint("photos/with/{userIdType}/{id}"), Authentication(false)]
    [DocUsesPageData, DocSummary("Gets photos depicting a user specified by their username or UUID")]
    [DocError(typeof(ApiNotFoundError), "The user cannot be found")]
    public ApiListResponse<ApiGamePhotoResponse> PhotosWithUser(RequestContext context,
        GameDatabaseContext database, DataContext dataContext,
        [DocSummary(SharedParamDescriptions.UserIdParam)] string id, 
        [DocSummary(SharedParamDescriptions.UserIdTypeParam)] string userIdType)
    {
        GameUser? user = database.GetUserByIdAndType(userIdType, id);
        if (user == null) return ApiNotFoundError.Instance;

        (int skip, int count) = context.GetPageData();

        DatabaseList<GamePhoto> photos = database.GetPhotosWithUser(user, count, skip);
        DatabaseList<ApiGamePhotoResponse> photosResponse = DatabaseListExtensions.FromOldList<ApiGamePhotoResponse, GamePhoto>(photos, dataContext);
        return photosResponse;
    }

    [ApiV3Endpoint("levels/id/{id}/photos"), Authentication(false)]
    [DocUsesPageData, DocSummary("Gets photos taken in a level by its id")]
    [DocError(typeof(ApiNotFoundError), "The level cannot be found")]
    public ApiListResponse<ApiGamePhotoResponse> PhotosInLevelById(RequestContext context, GameDatabaseContext database,
        IDataStore dataStore,
        [DocSummary("The ID of the level")] int id, DataContext dataContext)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.Instance;
        
        (int skip, int count) = context.GetPageData();

        DatabaseList<GamePhoto> photos = database.GetPhotosInLevel(level, count, skip);
        DatabaseList<ApiGamePhotoResponse> photosResponse = DatabaseListExtensions.FromOldList<ApiGamePhotoResponse, GamePhoto>(photos, dataContext);
        return photosResponse;
    }
    
    [ApiV3Endpoint("photos"), Authentication(false)]
    [DocUsesPageData, DocSummary("Get all photos taken recently")]
    public ApiListResponse<ApiGamePhotoResponse> RecentPhotos(RequestContext context, GameDatabaseContext database,
        IDataStore dataStore, DataContext dataContext)
    {
        (int skip, int count) = context.GetPageData();
        DatabaseList<GamePhoto> photos = database.GetRecentPhotos(count, skip);

        DatabaseList<ApiGamePhotoResponse> photosResponse = DatabaseListExtensions.FromOldList<ApiGamePhotoResponse, GamePhoto>(photos, dataContext);
        return photosResponse;
    }
    
    [ApiV3Endpoint("photos/id/{id}"), Authentication(false)]
    [DocUsesPageData, DocSummary("Get an individual photo by the photo's id")]
    [DocError(typeof(ApiNotFoundError), "The photo cannot be found")]
    public ApiResponse<ApiGamePhotoResponse> PhotoById(RequestContext context, GameDatabaseContext database,
        IDataStore dataStore, [DocSummary("The ID of the photo")] int id, DataContext dataContext)
    {
        GamePhoto? photo = database.GetPhotoById(id);
        if (photo == null) return ApiNotFoundError.Instance;
        
        return ApiGamePhotoResponse.FromOld(photo, dataContext);
    }
}