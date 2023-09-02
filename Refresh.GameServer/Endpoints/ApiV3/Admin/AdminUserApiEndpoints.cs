using AttribDoc.Attributes;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Documentation.Attributes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Admin;
using Refresh.GameServer.Extensions;
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

    [ApiV3Endpoint("admin/users"), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Gets all users with extended information.")]
    [DocUsesPageData]
    public ApiListResponse<ApiExtendedGameUserResponse> GetExtendedUsers(RequestContext context, GameDatabaseContext database)
    {
        (int skip, int count) = context.GetPageData(true);
        return DatabaseList<ApiExtendedGameUserResponse>.FromOldList<ApiExtendedGameUserResponse, GameUser>(database.GetUsers(count, skip));
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
    
    [ApiV3Endpoint("admin/users/uuid/{uuid}/planets"), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Retrieves the hashes of a user's planets. Gets user by their UUID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiResponse<ApiAdminUserPlanetsResponse> GetUserPlanetsByUuid(RequestContext context, GameDatabaseContext database, string uuid)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if (user == null) return ApiNotFoundError.UserMissingError;

        return new ApiAdminUserPlanetsResponse
        {
            Lbp2PlanetsHash = user.Lbp2PlanetsHash,
            Lbp3PlanetsHash = user.Lbp3PlanetsHash,
            VitaPlanetsHash = user.VitaPlanetsHash,
        };
    }
    
    [ApiV3Endpoint("admin/users/name/{username}/planets"), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Retrieves the hashes of a user's planets. Gets user by their username.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiResponse<ApiAdminUserPlanetsResponse> GetUserPlanetsByUsername(RequestContext context, GameDatabaseContext database, string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return ApiNotFoundError.UserMissingError;

        return new ApiAdminUserPlanetsResponse
        {
            Lbp2PlanetsHash = user.Lbp2PlanetsHash,
            Lbp3PlanetsHash = user.Lbp3PlanetsHash,
            VitaPlanetsHash = user.VitaPlanetsHash,
        };
    }
    
    [ApiV3Endpoint("admin/users/uuid/{uuid}/planets", Method.Delete), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Resets a user's planets. Gets user by their UUID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiOkResponse ResetUserPlanetsByUuid(RequestContext context, GameDatabaseContext database, string uuid)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if (user == null) return ApiNotFoundError.UserMissingError;

        database.ResetUserPlanets(user);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/users/name/{username}/planets", Method.Delete), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Resets a user's planets. Gets user by their username.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiOkResponse ResetUserPlanetsByUsername(RequestContext context, GameDatabaseContext database, string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return ApiNotFoundError.UserMissingError;

        database.ResetUserPlanets(user);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/users/uuid/{uuid}", Method.Delete), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Deletes a user user by their UUID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiOkResponse DeleteUserByUuid(RequestContext context, GameDatabaseContext database, string uuid)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if (user == null) return ApiNotFoundError.UserMissingError;

        database.DeleteUser(user);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/users/name/{username}", Method.Delete), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Deletes a user user by their UUID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiOkResponse DeleteUserByUsername(RequestContext context, GameDatabaseContext database, string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return ApiNotFoundError.UserMissingError;

        database.DeleteUser(user);
        return new ApiOkResponse();
    }
}