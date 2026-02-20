using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Http;
using Refresh.Core.Authentication.Permission;
using Refresh.Database;
using Refresh.Database.Models.Photos;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Documentation.Descriptions;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;

namespace Refresh.Interfaces.APIv3.Endpoints.Admin;

public class AdminPhotoApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("admin/users/{idType}/{id}/photos", HttpMethods.Delete), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Deletes all photos posted by a user. Gets user by their UUID or username.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiOkResponse DeletePhotosPostedByUser(RequestContext context, GameDatabaseContext database,
        [DocSummary(SharedParamDescriptions.UserIdParam)] string id, 
        [DocSummary(SharedParamDescriptions.UserIdTypeParam)] string idType)
    {
        GameUser? user = database.GetUserByIdAndType(idType, id);
        if (user == null) return ApiNotFoundError.UserMissingError;
        
        database.DeletePhotosPostedByUser(user);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/photos/id/{id}", HttpMethods.Delete), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Deletes a photo.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.PhotoMissingErrorWhen)]
    public ApiOkResponse DeletePhoto(RequestContext context, GameDatabaseContext database, int id)
    {
        GamePhoto? photo = database.GetPhotoById(id);
        if (photo == null) return ApiNotFoundError.PhotoMissingError;
        
        database.RemovePhoto(photo);
        return new ApiOkResponse();
    }
}