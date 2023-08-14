using AttribDoc.Attributes;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3.Admin;

public class AdminUserApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("admin/users/name/{username}"), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Gets a user by their name with extended information.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiResponse<ApiExtendedGameUserResponse> GetExtendedUserByUsername(RequestContext context, GameDatabaseContext database, string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return ApiNotFoundError.UserMissingError;

        return ApiExtendedGameUserResponse.FromOld(user);
    }

    [ApiV3Endpoint("admin/users/uuid/{uuid}"), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Gets a user by their UUID with extended information.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiResponse<ApiExtendedGameUserResponse> GetExtendedUserByUuid(RequestContext context, GameDatabaseContext database, string uuid)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if (user == null) return ApiNotFoundError.UserMissingError;

        return ApiExtendedGameUserResponse.FromOld(user);
    }

    private static ApiOkResponse ResetUserPassword(GameDatabaseContext database, ApiResetUserPasswordRequest body, GameUser user)
    {
        if (body.PasswordSha512.Length != 128 || !AuthenticationApiEndpoints.Sha512Regex().IsMatch(body.PasswordSha512))
            return new ApiValidationError("Password is definitely not SHA512. Please hash the password.");
        
        string? passwordBcrypt = BC.HashPassword(body.PasswordSha512, AuthenticationApiEndpoints.WorkFactor);
        if (passwordBcrypt == null) return new ApiInternalError("Could not BCrypt the given password.");
        
        database.SetUserPassword(user, passwordBcrypt, true);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("admin/users/uuid/{uuid}/resetPassword", Method.Put), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Reset's a user password by their UUID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocRequestBody(typeof(ApiResetUserPasswordRequest))]
    public ApiOkResponse ResetUserPasswordByUuid(RequestContext context, GameDatabaseContext database, ApiResetUserPasswordRequest body, string uuid)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if (user == null) return ApiNotFoundError.UserMissingError;

        return ResetUserPassword(database, body, user);
    }
    
    [ApiV3Endpoint("admin/users/name/{username}/resetPassword", Method.Put), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Reset's a user password by their username.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocRequestBody(typeof(ApiResetUserPasswordRequest))]
    public ApiOkResponse ResetUserPasswordByUsername(RequestContext context, GameDatabaseContext database, ApiResetUserPasswordRequest body, string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return ApiNotFoundError.UserMissingError;

        return ResetUserPassword(database, body, user);
    }
}