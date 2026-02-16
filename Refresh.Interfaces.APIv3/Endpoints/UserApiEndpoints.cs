using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Bunkum.Protocols.Http;
using Refresh.Common.Constants;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Core.Services;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Pins;
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

    // TODO: Also allow specifying user by username
    [ApiV3Endpoint("users/uuid/{uuid}/heart", HttpMethods.Post)]
    [DocSummary("Hearts a user by their UUID")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiOkResponse HeartUserByUuid(RequestContext context, GameDatabaseContext database,
        [DocSummary("The UUID of the user")] string uuid, DataContext dataContext, GameUser user)
    {
        GameUser? target = database.GetUserByUuid(uuid);
        if(target == null) return ApiNotFoundError.UserMissingError;
        
        bool success = database.FavouriteUser(target, user);

        // Only give pin if the user was hearted without having already been hearted.
        // Won't protect against spam, but this way the pin objective is more accurately implemented.
        if (success)
            database.IncrementUserPinProgress((long)ServerPins.HeartPlayerOnWebsite, 1, user, false, TokenPlatform.Website);

        return new ApiOkResponse();
    }

    [ApiV3Endpoint("users/uuid/{uuid}/unheart", HttpMethods.Post)]
    [DocSummary("Unhearts a user by their UUID")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiOkResponse UnheartUserByUuid(RequestContext context, GameDatabaseContext database,
        [DocSummary("The UUID of the user")] string uuid, DataContext dataContext, GameUser user)
    {
        GameUser? target = database.GetUserByUuid(uuid);
        if(target == null) return ApiNotFoundError.UserMissingError;
        
        database.UnfavouriteUser(target, user);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("users/me"), MinimumRole(GameUserRole.Restricted)]
    [DocSummary("Returns your own user, provided you are authenticated")]
    [DocError(typeof(ApiAuthenticationError), "You are not authenticated")]
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
        // If any icon is requested to be reset, force its hash to be a specific value, 
        // to not allow uncontrolled values which would still count as blank/empty hash (e.g. unlimited whitespaces)
        if (body.IconHash != null)
        {
            if (body.IconHash.IsBlankHash())
            {
                body.IconHash = "0";
            }
            else if (database.GetAssetFromHash(body.IconHash) == null)
            {
                return ApiNotFoundError.Instance;
            }
        }

        if (body.VitaIconHash != null)
        {
            if (body.VitaIconHash.IsBlankHash())
            {
                body.VitaIconHash = "0";
            }
            else if (database.GetAssetFromHash(body.VitaIconHash) == null)
            {
                return ApiNotFoundError.Instance;
            }
        }

        if (body.BetaIconHash != null)
        {
            if (body.BetaIconHash.IsBlankHash())
            {
                body.BetaIconHash = "0";
            }
            else if (database.GetAssetFromHash(body.BetaIconHash) == null)
            {
                return ApiNotFoundError.Instance;
            }
        }

        if (body.EmailAddress != null && !smtpService.CheckEmailDomainValidity(body.EmailAddress))
            return ApiValidationError.EmailDoesNotActuallyExistError;
        
        // Trim description
        if (body.Description != null && body.Description.Length > UgcLimits.DescriptionLimit)
            body.Description = body.Description[..UgcLimits.DescriptionLimit];

        database.UpdateUserData(user, body);
        return ApiExtendedGameUserResponse.FromOld(user, dataContext);
    }
}