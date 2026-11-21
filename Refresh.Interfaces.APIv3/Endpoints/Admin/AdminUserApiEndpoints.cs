using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Bunkum.Protocols.Http;
using Refresh.Common.Constants;
using Refresh.Common.Verification;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Types.Categories;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Moderation;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;
using Refresh.Interfaces.APIv3.Documentation.Attributes;
using Refresh.Interfaces.APIv3.Documentation.Descriptions;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Admin;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Categories;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;
using Refresh.Interfaces.APIv3.Extensions;

namespace Refresh.Interfaces.APIv3.Endpoints.Admin;

using BC = BCrypt.Net.BCrypt;

public class AdminUserApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("admin/users/uuid/{uuid}")]
    [ApiV3Endpoint("admin/users/name/{username}")]
    [ApiV3Endpoint("admin/users/username/{username}")]
    [MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Gets a user by their UUID or name with extended information.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiResponse<ApiExtendedGameUserResponse> GetExtendedUser(RequestContext context,
        GameDatabaseContext database, DataContext dataContext,
        string? uuid, string? username)
    {
        GameUser? user = database.GetUserByUuidOrUsername(uuid, username);
        if (user == null) return ApiNotFoundError.UserMissingError;

        return ApiExtendedGameUserResponse.FromOld(user, dataContext);
    }

    [ApiV3Endpoint("admin/userCategories"), MinimumRole(GameUserRole.Moderator)]
    [ClientCacheResponse(1800)] // cache for half an hour
    [DocQueryParam("includePreviews", "If true, a single user will be added to each category representing a user from that category. False by default.")]
    [DocSummary("Gets all staff-only user categories.")]
    public ApiListResponse<ApiCategoryResponse> GetAdminUserCategories(RequestContext context,
        DataContext dataContext, CategoryService categories, string route)
    {
        bool result = bool.TryParse(context.QueryString.Get("includePreviews") ?? "false", out bool includePreviews);
        if (!result) return ApiValidationError.BooleanParseError; // TODO actually name the failed bool param
        
        // Ignore the PermitShowingOnlineUsers config option, since this is mods-only
        
        IEnumerable<ApiCategoryResponse> resp;
        if (includePreviews) resp = ApiCategoryResponse.FromOldList(categories.AdminUserCategories, context, dataContext);
        else resp = ApiCategoryResponse.FromOldList(categories.AdminUserCategories, dataContext);

        // No need for extended info for previews (yet)
        return new ApiListResponse<ApiCategoryResponse>(resp);
    }

    [ApiV3Endpoint("admin/userCategories/{route}"), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Gets all users with extended information from a staff-only category.")]
    [DocUsesPageData]
    public ApiListResponse<ApiExtendedGameUserResponse> GetExtendedUsersViaCategory(RequestContext context,
        DataContext dataContext, CategoryService categories, GameUser user, string route)
    {
        (int skip, int count) = context.GetPageData();
        
        DatabaseList<GameUser>? list = categories.AdminUserCategories
            .FirstOrDefault(c => c.ApiRoute.StartsWith(route))?
            .Fetch(context, skip, count, dataContext, LevelFilterSettings.FromApiRequest(context), user)?
            .Users;

        if (list == null) return ApiNotFoundError.Instance;

        DatabaseList<ApiExtendedGameUserResponse> levels = DatabaseListExtensions.FromOldList<ApiExtendedGameUserResponse, GameUser>(list, dataContext);
        return levels;
    }

    [ApiV3Endpoint("admin/users"), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Gets the newest users with extended information. Redirects to /admin/userCategories/newest now.")]
    [DocUsesPageData]
    public ApiListResponse<ApiExtendedGameUserResponse> GetExtendedUsers(RequestContext context, DataContext dataContext, 
        GameUser user, CategoryService categories)
    {
        return this.GetExtendedUsersViaCategory(context, dataContext, categories, user, "newest");
    }

    [ApiV3Endpoint("admin/previousUsernames/byName/{username}"), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Gets all users (with extended information) who have once used the specified username.")]
    [DocUsesPageData]
    public ApiListResponse<ApiExtendedPreviousUsernameResponse> GetExtendedPreviousUsernamesByUsername(RequestContext context,
        GameDatabaseContext database, IDataStore dataStore, DataContext dataContext, string username)
    {
        (int skip, int count) = context.GetPageData();
        DatabaseList<PreviousUsername> previousNames = database.GetPreviousUsernameRecordsByName(username, skip, count);
        return DatabaseListExtensions.FromOldList<ApiExtendedPreviousUsernameResponse, PreviousUsername>(previousNames, dataContext);
    }

    [ApiV3Endpoint("admin/previousUsernames/byUser/uuid/{uuid}")]
    [ApiV3Endpoint("admin/previousUsernames/byUser/name/{username}")]
    [ApiV3Endpoint("admin/previousUsernames/byUser/username/{username}")]
    [MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Gets all previous usernames which have once been used by the specified user.")]
    [DocUsesPageData]
    public ApiListResponse<ApiExtendedPreviousUsernameResponse> GetExtendedPreviousUsernamesByUser(RequestContext context,
        GameDatabaseContext database, DataContext dataContext, string? uuid, string? username)
    {
        GameUser? user = database.GetUserByUuidOrUsername(uuid, username);
        if (user == null) return ApiNotFoundError.UserMissingError;
        
        (int skip, int count) = context.GetPageData();
        DatabaseList<PreviousUsername> previousNames = database.GetPreviousUsernameRecordsByUser(user, skip, count);
        return DatabaseListExtensions.FromOldList<ApiExtendedPreviousUsernameResponse, PreviousUsername>(previousNames, dataContext);
    }

    [ApiV3Endpoint("admin/users/uuid/{uuid}/resetPassword", HttpMethods.Put)]
    [ApiV3Endpoint("admin/users/name/{username}/resetPassword", HttpMethods.Put)]
    [ApiV3Endpoint("admin/users/username/{username}/resetPassword", HttpMethods.Put)]
    [MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Resets a user's password by their UUID or username.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.MayNotModifyUserDueToLowRoleErrorWhen)]
    [DocRequestBody(typeof(ApiResetUserPasswordRequest))]
    public ApiOkResponse ResetUserPassword(RequestContext context, GameDatabaseContext database, ApiResetUserPasswordRequest body, GameUser user,
        string? uuid, string? username)
    {
        GameUser? targetUser = database.GetUserByUuidOrUsername(uuid, username);
        if (targetUser == null) return ApiNotFoundError.UserMissingError;

        if (!user.MayModifyUser(targetUser))
            return ApiValidationError.MayNotModifyUserDueToLowRoleError;

        if (body.PasswordSha512.Length != 128 || !CommonPatterns.Sha512Regex().IsMatch(body.PasswordSha512))
            return new ApiValidationError("Password is definitely not SHA512. Please hash the password.");
        
        string? passwordBcrypt = BC.HashPassword(body.PasswordSha512, AuthenticationApiEndpoints.WorkFactor);
        if (passwordBcrypt == null) return new ApiInternalError("Could not BCrypt the given password.");
        
        database.SetUserPassword(targetUser, passwordBcrypt, true);
        return new ApiOkResponse();
    }
    
    // TODO: Users should be able to retrieve and reset their own planets
    [ApiV3Endpoint("admin/users/uuid/{uuid}/planets")]
    [ApiV3Endpoint("admin/users/name/{username}/planets")]
    [ApiV3Endpoint("admin/users/username/{username}/planets")]
    [MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Retrieves the hashes of a user's planets and whether they're modded. Gets user by their UUID or username.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiResponse<ApiAdminUserPlanetsResponse> GetUserPlanets(RequestContext context, GameDatabaseContext database,
        string? uuid, string? username)
    {
        GameUser? targetUser = database.GetUserByUuidOrUsername(uuid, username);
        if (targetUser == null) return ApiNotFoundError.UserMissingError;

        return new ApiAdminUserPlanetsResponse
        {
            Lbp2PlanetsHash = targetUser.Lbp2PlanetsHash,
            Lbp3PlanetsHash = targetUser.Lbp3PlanetsHash,
            VitaPlanetsHash = targetUser.VitaPlanetsHash,
            BetaPlanetsHash = targetUser.BetaPlanetsHash,
            AreLbp2PlanetsModded = targetUser.AreLbp2PlanetsModded,
            AreLbp3PlanetsModded = targetUser.AreLbp3PlanetsModded,
            AreVitaPlanetsModded = targetUser.AreVitaPlanetsModded,
            AreBetaPlanetsModded = targetUser.AreBetaPlanetsModded,
        };
    }
    
    [ApiV3Endpoint("admin/users/uuid/{uuid}/planets", HttpMethods.Delete)]
    [ApiV3Endpoint("admin/users/name/{username}/planets", HttpMethods.Delete)]
    [ApiV3Endpoint("admin/users/username/{username}/planets", HttpMethods.Delete)]
    [MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Resets a user's planets. Gets user by their UUID or username.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.MayNotModifyUserDueToLowRoleErrorWhen)]
    public ApiOkResponse ResetUserPlanets(RequestContext context, GameDatabaseContext database, GameUser user,
        string? uuid, string? username)
    {
        GameUser? targetUser = database.GetUserByUuidOrUsername(uuid, username);
        if (targetUser == null) return ApiNotFoundError.UserMissingError;

        if (!user.MayModifyUser(targetUser))
            return ApiValidationError.MayNotModifyUserDueToLowRoleError;

        database.ResetUserPlanets(targetUser);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("admin/users/uuid/{uuid}", HttpMethods.Patch)]
    [ApiV3Endpoint("admin/users/name/{username}", HttpMethods.Patch)]
    [ApiV3Endpoint("admin/users/username/{username}", HttpMethods.Patch)]
    [MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Updates the specified user's profile with the given data")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.MayNotModifyUserDueToLowRoleErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.MayNotOverwriteRoleErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.RoleMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.WrongRoleUpdateMethodErrorWhen)]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.IconMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.InvalidUsernameErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.UsernameTakenErrorWhen)]
    public ApiResponse<ApiExtendedGameUserResponse> UpdateUser(RequestContext context, GameDatabaseContext database,
        GameUser user, ApiAdminUpdateUserRequest body, DataContext dataContext,
        string? uuid, string? username)
    {
        GameUser? targetUser = database.GetUserByUuidOrUsername(uuid, username);

        if (targetUser == null)
            return ApiNotFoundError.UserMissingError;

        if (!user.MayModifyUser(targetUser))
            return ApiValidationError.MayNotModifyUserDueToLowRoleError;

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

        (body.IconHash, ApiError? mainIconError) = body.IconHash.ValidateIcon(dataContext);
        if (mainIconError != null) return mainIconError;

        (body.VitaIconHash, ApiError? vitaIconError) = body.VitaIconHash.ValidateIcon(dataContext);
        if (vitaIconError != null) return vitaIconError;

        (body.BetaIconHash, ApiError? betaIconError) = body.BetaIconHash.ValidateIcon(dataContext);
        if (betaIconError != null) return betaIconError;

        // Do nothing if the username entered is actually the same as the one already set
        if (body.Username != null && body.Username != targetUser.Username) 
        {
            if (!body.Username.StartsWith(SystemUsers.SystemPrefix) && !database.IsUsernameValid(body.Username))
                return new ApiValidationError(ApiValidationError.InvalidUsernameErrorWhen
                    + " Are you sure you used a PSN/RPCN username, or prepended it with ! if it's a fake user?");
            
            if (database.IsUsernameTaken(body.Username))
                return ApiValidationError.UsernameTakenError;
            
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
    
    [ApiV3Endpoint("admin/users/uuid/{uuid}", HttpMethods.Delete)]
    [ApiV3Endpoint("admin/users/name/{username}", HttpMethods.Delete)]
    [ApiV3Endpoint("admin/users/username/{username}", HttpMethods.Delete)]
    [MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Deletes a user by their UUID or username.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.MayNotModifyUserDueToLowRoleErrorWhen)]
    public ApiOkResponse DeleteUser(RequestContext context, GameDatabaseContext database, GameUser user,
        string? uuid, string? username)
    {
        GameUser? targetUser = database.GetUserByUuidOrUsername(uuid, username);
        if (targetUser == null) return ApiNotFoundError.UserMissingError;

        if (!user.MayModifyUser(targetUser))
            return ApiValidationError.MayNotModifyUserDueToLowRoleError;

        database.DeleteUser(targetUser);
        return new ApiOkResponse();
    }
}