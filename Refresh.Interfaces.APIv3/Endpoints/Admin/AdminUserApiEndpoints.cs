using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Bunkum.Protocols.Http;
using Refresh.Common.Constants;
using Refresh.Common.Verification;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Moderation;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Documentation.Attributes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Admin;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;
using Refresh.Interfaces.APIv3.Extensions;

namespace Refresh.Interfaces.APIv3.Endpoints.Admin;

using BC = BCrypt.Net.BCrypt;

public class AdminUserApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("admin/users/name/{username}"), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Gets a user by their name with extended information.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiResponse<ApiExtendedGameUserResponse> GetExtendedUserByUsername(RequestContext context,
        GameDatabaseContext database, string username, IDataStore dataStore, DataContext dataContext)
    {
        GameUser? user = database.GetUserByUsername(username, false);
        if (user == null) return ApiNotFoundError.UserMissingError;

        return ApiExtendedGameUserResponse.FromOld(user, dataContext);
    }

    [ApiV3Endpoint("admin/users/uuid/{uuid}"), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Gets a user by their UUID with extended information.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiResponse<ApiExtendedGameUserResponse> GetExtendedUserByUuid(RequestContext context,
        GameDatabaseContext database, string uuid, IDataStore dataStore, DataContext dataContext)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if (user == null) return ApiNotFoundError.UserMissingError;

        return ApiExtendedGameUserResponse.FromOld(user, dataContext);
    }

    [ApiV3Endpoint("admin/users"), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Gets all users with extended information.")]
    [DocUsesPageData]
    public ApiListResponse<ApiExtendedGameUserResponse> GetExtendedUsers(RequestContext context,
        GameDatabaseContext database, IDataStore dataStore, DataContext dataContext)
    {
        (int skip, int count) = context.GetPageData();
        DatabaseList<ApiExtendedGameUserResponse> list = DatabaseListExtensions.FromOldList<ApiExtendedGameUserResponse, GameUser>(database.GetUsers(count, skip), dataContext);
        return list;
    }

    private static ApiOkResponse ResetUserPassword(GameDatabaseContext database, ApiResetUserPasswordRequest body, GameUser user)
    {
        if (body.PasswordSha512.Length != 128 || !CommonPatterns.Sha512Regex().IsMatch(body.PasswordSha512))
            return new ApiValidationError("Password is definitely not SHA512. Please hash the password.");
        
        string? passwordBcrypt = BC.HashPassword(body.PasswordSha512, AuthenticationApiEndpoints.WorkFactor);
        if (passwordBcrypt == null) return new ApiInternalError("Could not BCrypt the given password.");
        
        database.SetUserPassword(user, passwordBcrypt, true);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("admin/users/uuid/{uuid}/resetPassword", HttpMethods.Put), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Resets a user's password by their UUID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocRequestBody(typeof(ApiResetUserPasswordRequest))]
    public ApiOkResponse ResetUserPasswordByUuid(RequestContext context, GameDatabaseContext database, ApiResetUserPasswordRequest body, string uuid)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if (user == null) return ApiNotFoundError.UserMissingError;

        return ResetUserPassword(database, body, user);
    }
    
    [ApiV3Endpoint("admin/users/name/{username}/resetPassword", HttpMethods.Put), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Resets a user's password by their username.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocRequestBody(typeof(ApiResetUserPasswordRequest))]
    public ApiOkResponse ResetUserPasswordByUsername(RequestContext context, GameDatabaseContext database, ApiResetUserPasswordRequest body, string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return ApiNotFoundError.UserMissingError;

        return ResetUserPassword(database, body, user);
    }
    
    // TODO: Users should be able to retrieve and reset their own planets
    [ApiV3Endpoint("admin/users/uuid/{uuid}/planets"), MinimumRole(GameUserRole.Moderator)]
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
            BetaPlanetsHash = user.BetaPlanetsHash,
            AreLbp2PlanetsModded = user.AreLbp2PlanetsModded,
            AreLbp3PlanetsModded = user.AreLbp3PlanetsModded,
            AreVitaPlanetsModded = user.AreVitaPlanetsModded,
            AreBetaPlanetsModded = user.AreBetaPlanetsModded,
        };
    }
    
    [ApiV3Endpoint("admin/users/name/{username}/planets"), MinimumRole(GameUserRole.Moderator)]
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
            BetaPlanetsHash = user.BetaPlanetsHash,
            AreLbp2PlanetsModded = user.AreLbp2PlanetsModded,
            AreLbp3PlanetsModded = user.AreLbp3PlanetsModded,
            AreVitaPlanetsModded = user.AreVitaPlanetsModded,
            AreBetaPlanetsModded = user.AreBetaPlanetsModded,
        };
    }
    
    [ApiV3Endpoint("admin/users/uuid/{uuid}/planets", HttpMethods.Delete), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Resets a user's planets. Gets user by their UUID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiOkResponse ResetUserPlanetsByUuid(RequestContext context, GameDatabaseContext database, string uuid)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if (user == null) return ApiNotFoundError.UserMissingError;

        database.ResetUserPlanets(user);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/users/name/{username}/planets", HttpMethods.Delete), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Resets a user's planets. Gets user by their username.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiOkResponse ResetUserPlanetsByUsername(RequestContext context, GameDatabaseContext database, string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return ApiNotFoundError.UserMissingError;

        database.ResetUserPlanets(user);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("admin/users/{idType}/{id}", HttpMethods.Patch), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Updates the specified user's profile with the given data")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.MayNotOverwriteRoleErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.RoleMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.WrongRoleUpdateMethodErrorWhen)]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.IconMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.InvalidUsernameErrorWhen)]
    public ApiResponse<ApiExtendedGameUserResponse> UpdateUser(RequestContext context, GameDatabaseContext database,
        GameUser user, ApiAdminUpdateUserRequest body, DataContext dataContext, 
        [DocSummary("The type of identifier used to look up the user. Can be either 'uuid' or 'username'.")] string idType, 
        [DocSummary("The UUID or username of the user, depending on the specified ID type.")] string id)
    {
        GameUser? targetUser = database.GetUserByIdAndType(idType, id);

        if (targetUser == null)
            return ApiNotFoundError.UserMissingError;

        // Only admins may edit anyone's role.
        // TODO: Maybe moderators should also be able to set roles, but only for users below them, and to roles below them?
        if (body.Role != null)
        {
            if (user.Role < GameUserRole.Admin)
                return ApiValidationError.MayNotOverwriteRoleError;

            if (!Enum.IsDefined(typeof(GameUserRole), body.Role))
                return ApiValidationError.RoleMissingError;

            // All roles below regular user are special and must be given using different endpoints because they require extra information.
            // Incase the implementation of #286 requires a guest role, that one will very likely be below User aswell, and it should also not
            // be assignable with this endpoint (when should a user ever be demoted to a temporary guest?)
            if (body.Role < GameUserRole.User)
                return ApiValidationError.WrongRoleUpdateMethodError;
        }

        if (body.IconHash != null)
        {
            if (body.IconHash.IsBlankHash())
                body.IconHash = "0";
            else if (database.GetAssetFromHash(body.IconHash) == null)
                return ApiNotFoundError.IconMissingError;
        }

        if (body.VitaIconHash != null)
        {
            if (body.VitaIconHash.IsBlankHash())
                body.VitaIconHash = "0";
            else if (database.GetAssetFromHash(body.VitaIconHash) == null)
                return ApiNotFoundError.IconMissingError;
        }

        if (body.BetaIconHash != null)
        {
            if (body.BetaIconHash.IsBlankHash())
                body.BetaIconHash = "0";
            else if (database.GetAssetFromHash(body.BetaIconHash) == null)
                return ApiNotFoundError.IconMissingError;
        }

        // Do nothing if the username entered is actually the same as the one already set
        if (body.Username != null && body.Username != targetUser.Username) 
        {
            if (!body.Username.StartsWith(SystemUsers.SystemPrefix) && !database.IsUsernameValid(body.Username))
                return new ApiValidationError(ApiValidationError.InvalidUsernameErrorWhen
                    + " Are you sure you used a PSN/RPCN username, or prepended it with ! if it's a fake user?");
            
            if (database.IsUsernameTaken(body.Username))
                return new ApiValidationError("This username is already taken!");
            
            database.RenameUser(targetUser, body.Username);
        }

        // Trim description
        if (body.Description != null && body.Description.Length > UgcLimits.DescriptionLimit)
            body.Description = body.Description[..UgcLimits.DescriptionLimit];

        database.UpdateUserData(targetUser, body);
        // TODO: In ApiV4, moderation actions should also provide reasons
        database.CreateModerationAction(targetUser, ModerationActionType.UserModification, user, "");

        return ApiExtendedGameUserResponse.FromOld(targetUser, dataContext);
    }
    
    [ApiV3Endpoint("admin/users/uuid/{uuid}", HttpMethods.Delete), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Deletes a user user by their UUID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiOkResponse DeleteUserByUuid(RequestContext context, GameDatabaseContext database, string uuid)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if (user == null) return ApiNotFoundError.UserMissingError;

        database.DeleteUser(user);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/users/name/{username}", HttpMethods.Delete), MinimumRole(GameUserRole.Moderator)]
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