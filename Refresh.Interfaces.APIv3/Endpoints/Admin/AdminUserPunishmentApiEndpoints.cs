using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Http;
using Refresh.Core.Authentication.Permission;
using Refresh.Database;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;

namespace Refresh.Interfaces.APIv3.Endpoints.Admin;

public class AdminUserPunishmentApiEndpoints : EndpointGroup
{
    private static ApiOkResponse BanUser(GameUser? user, GameDatabaseContext database, ApiPunishUserRequest body)
    {
        if (user == null) return ApiNotFoundError.UserMissingError;
        
        database.BanUser(user, body.Reason, body.ExpiryDate);
        return new ApiOkResponse();
    }
    
    private static ApiOkResponse RestrictUser(GameUser? user, GameDatabaseContext database, ApiPunishUserRequest body)
    {
        if (user == null) return ApiNotFoundError.UserMissingError;
        
        database.RestrictUser(user, body.Reason, body.ExpiryDate);
        return new ApiOkResponse();
    }
    
    private static ApiOkResponse PardonUser(GameUser? user, GameDatabaseContext database)
    {
        if (user == null) return ApiNotFoundError.UserMissingError;
        
        database.SetUserRole(user, GameUserRole.User);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("admin/users/name/{username}/ban", HttpMethods.Post), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Bans a user for the specified reason until the given date.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocRequestBody(typeof(ApiPunishUserRequest))]
    public ApiOkResponse BanByUsername(RequestContext context, GameDatabaseContext database, string username, ApiPunishUserRequest body) 
        => BanUser(database.GetUserByUsername(username), database, body);
    
    [ApiV3Endpoint("admin/users/uuid/{uuid}/ban", HttpMethods.Post), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Bans a user for the specified reason until the given date.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocRequestBody(typeof(ApiPunishUserRequest))]
    public ApiOkResponse BanByUuid(RequestContext context, GameDatabaseContext database, string uuid, ApiPunishUserRequest body) 
        => BanUser(database.GetUserByUuid(uuid), database, body);
    
    [ApiV3Endpoint("admin/users/name/{username}/restrict", HttpMethods.Post), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Restricts a user for the specified reason until the given date.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocRequestBody(typeof(ApiPunishUserRequest))]
    public ApiOkResponse RestrictByUsername(RequestContext context, GameDatabaseContext database, string username, ApiPunishUserRequest body) 
        => RestrictUser(database.GetUserByUsername(username), database, body);
    
    [ApiV3Endpoint("admin/users/uuid/{uuid}/restrict", HttpMethods.Post), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Restricts a user for the specified reason until the given date.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocRequestBody(typeof(ApiPunishUserRequest))]
    public ApiOkResponse RestrictByUuid(RequestContext context, GameDatabaseContext database, string uuid, ApiPunishUserRequest body) 
        => RestrictUser(database.GetUserByUuid(uuid), database, body);
    
    [ApiV3Endpoint("admin/users/name/{username}/pardon", HttpMethods.Post), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Pardons all punishments for the given user.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiOkResponse PardonByUsername(RequestContext context, GameDatabaseContext database, string username) 
        => PardonUser(database.GetUserByUsername(username), database);
    
    [ApiV3Endpoint("admin/users/uuid/{uuid}/pardon", HttpMethods.Post), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Pardons all punishments for the given user.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiOkResponse PardonByUuid(RequestContext context, GameDatabaseContext database, string uuid) 
        => PardonUser(database.GetUserByUuid(uuid), database);
}