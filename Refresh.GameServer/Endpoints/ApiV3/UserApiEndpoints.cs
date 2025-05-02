using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Configuration;
using Refresh.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Users;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;
using GameDatabaseContext = Refresh.Database.GameDatabaseContext;

namespace Refresh.GameServer.Endpoints.ApiV3;

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
        GameUser? user = database.GetUserByUsername(username);
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
    public ApiResponse<ApiExtendedGameUserResponse> GetMyUser(RequestContext context, GameUser user,
        GameDatabaseContext database, IDataStore dataStore, DataContext dataContext)
        => ApiExtendedGameUserResponse.FromOld(user, dataContext);
    
    [ApiV3Endpoint("users/me", HttpMethods.Patch)]
    [DocSummary("Updates your profile with the given data")]
    public ApiResponse<ApiExtendedGameUserResponse> UpdateUser(RequestContext context, GameDatabaseContext database,
        GameUser user, ApiUpdateUserRequest body, IDataStore dataStore, DataContext dataContext, IntegrationConfig integrationConfig,
        SmtpService smtpService)
    {
        if (body.IconHash != null && database.GetAssetFromHash(body.IconHash) == null)
            return ApiNotFoundError.Instance;

        if (body.EmailAddress != null && !smtpService.CheckEmailDomainValidity(body.EmailAddress))
            return ApiValidationError.EmailDoesNotActuallyExistError;

        database.UpdateUserData(user, body);
        return ApiExtendedGameUserResponse.FromOld(user, dataContext);
    }
}