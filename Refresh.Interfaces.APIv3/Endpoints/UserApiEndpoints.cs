using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Bunkum.Protocols.Http;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Core.Services;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;

namespace Refresh.Interfaces.APIv3.Endpoints;

public class UserApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("users/name/{username}"), Authentication(false)]
    [DocSummary("Tries to find a user by the username")]
    [DocError(typeof(ApiNotFoundError), "The user cannot be found")]
    public ApiResponse<ApiGameUserResponse> GetUserByName(RequestContext context, GameDatabaseContext database,
        IDataStore dataStore,
        [DocSummary("The username of the user")]
        string username, DataContext dataContext)
    {
        GameUser? user = database.GetUserByUsername(username, false);
        if(user == null) return ApiNotFoundError.UserMissingError;
        
        return ApiGameUserResponse.FromOld(user, dataContext);
    }
    
    [ApiV3Endpoint("users/uuid/{uuid}"), Authentication(false)]
    [DocSummary("Tries to find a user by the UUID")]
    [DocError(typeof(ApiNotFoundError), "The user cannot be found")]
    public ApiResponse<ApiGameUserResponse> GetUserByUuid(RequestContext context, GameDatabaseContext database,
        IDataStore dataStore,
        [DocSummary("The UUID of the user")] string uuid, DataContext dataContext)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if(user == null) return ApiNotFoundError.UserMissingError;
        
        return ApiGameUserResponse.FromOld(user, dataContext);
    }
    
    [ApiV3Endpoint("users/me"), MinimumRole(GameUserRole.Restricted)]
    [DocSummary("Returns your own user, provided you are authenticated")]
    [DocError(typeof(ApiAuthenticationError), "The user is not authenticated")]
    public ApiResponse<ApiExtendedGameUserResponse> GetMyUser(RequestContext context, GameUser? user,
        GameDatabaseContext database, IDataStore dataStore, DataContext dataContext)
    {
        if (user == null) return ApiAuthenticationError.NotAuthenticated;
        return ApiExtendedGameUserResponse.FromOld(user, dataContext);
    }
    
    [ApiV3Endpoint("users/me", HttpMethods.Patch)]
    [DocSummary("Updates your profile with the given data")]
    public ApiResponse<ApiExtendedGameUserResponse> UpdateUser(RequestContext context, GameDatabaseContext database,
        GameUser user, ApiUpdateUserRequest body, IDataStore dataStore, DataContext dataContext, IntegrationConfig integrationConfig,
        SmtpService smtpService)
    {
        if (body.IconHash != null && database.GetAssetFromHash(body.IconHash) == null)
            return ApiNotFoundError.Instance;
        
        if (body.VitaIconHash != null && database.GetAssetFromHash(body.VitaIconHash) == null)
            return ApiNotFoundError.Instance;
        
        if (body.BetaIconHash != null && database.GetAssetFromHash(body.BetaIconHash) == null)
            return ApiNotFoundError.Instance;

        if (body.EmailAddress != null && !smtpService.CheckEmailDomainValidity(body.EmailAddress))
            return ApiValidationError.EmailDoesNotActuallyExistError;

        database.UpdateUserData(user, body);
        return ApiExtendedGameUserResponse.FromOld(user, dataContext);
    }
}