using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Http;
using Refresh.Core.Authentication.Permission;
using Refresh.Database;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Documentation.Descriptions;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;

namespace Refresh.Interfaces.APIv3.Endpoints.Admin;

public class AdminUserPunishmentApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("admin/users/uuid/{uuid}/ban", HttpMethods.Post)]
    [ApiV3Endpoint("admin/users/name/{username}/ban", HttpMethods.Post)]
    [ApiV3Endpoint("admin/users/username/{username}/ban", HttpMethods.Post)]
    [MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Bans a user for the specified reason until the given date.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.MayNotModifyUserDueToLowRoleErrorWhen)]
    [DocRequestBody(typeof(ApiPunishUserRequest))]
    public ApiOkResponse BanUser(RequestContext context, GameDatabaseContext database, ApiPunishUserRequest body, GameUser user,
        string? uuid, string? username)
    {
        GameUser? targetUser = database.GetUserByUuidOrUsername(uuid, username);
        if (targetUser == null) return ApiNotFoundError.UserMissingError;
        
        if (!user.MayModifyUser(targetUser))
            return ApiValidationError.MayNotModifyUserDueToLowRoleError;

        database.BanUser(targetUser, body.Reason, body.ExpiryDate);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/users/uuid/{uuid}/restrict", HttpMethods.Post)]
    [ApiV3Endpoint("admin/users/name/{username}/restrict", HttpMethods.Post)]
    [ApiV3Endpoint("admin/users/username/{username}/restrict", HttpMethods.Post)]
    [MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Restricts a user for the specified reason until the given date.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.MayNotModifyUserDueToLowRoleErrorWhen)]
    [DocRequestBody(typeof(ApiPunishUserRequest))]
    public ApiOkResponse RestrictUser(RequestContext context, GameDatabaseContext database, ApiPunishUserRequest body, GameUser user,
        string? uuid, string? username)
    {
        GameUser? targetUser = database.GetUserByUuidOrUsername(uuid, username);
        if (targetUser == null) return ApiNotFoundError.UserMissingError;
        
        if (!user.MayModifyUser(targetUser))
            return ApiValidationError.MayNotModifyUserDueToLowRoleError;

        database.RestrictUser(targetUser, body.Reason, body.ExpiryDate);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/users/uuid/{uuid}/pardon", HttpMethods.Post)]
    [ApiV3Endpoint("admin/users/name/{username}/pardon", HttpMethods.Post)]
    [ApiV3Endpoint("admin/users/username/{username}/pardon", HttpMethods.Post)]
    [MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Pardons all punishments for the given user.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.UserIsAlreadyPardonedErrorWhen)]
    public ApiOkResponse PardonUser(RequestContext context, GameDatabaseContext database, GameUser user,
        string? uuid, string? username)
    {
        GameUser? targetUser = database.GetUserByUuidOrUsername(uuid, username);
        if (targetUser == null) return ApiNotFoundError.UserMissingError;

        if (targetUser.Role > GameUserRole.Restricted)
            return ApiValidationError.UserIsAlreadyPardonedError;
        
        database.SetUserRole(targetUser, GameUserRole.User);
        return new ApiOkResponse();
    }
}