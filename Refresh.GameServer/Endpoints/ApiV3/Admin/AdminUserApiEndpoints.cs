using AttribDoc.Attributes;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3.Admin;

public class AdminUserApiEndpoints : EndpointGroup
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

    [ApiV3Endpoint("admin/users/name/{username}/ban", Method.Post), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Bans a user for the specified reason until the given date.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocRequestBody(typeof(ApiPunishUserRequest))]
    public ApiOkResponse BanByUsername(RequestContext context, GameDatabaseContext database, string username, ApiPunishUserRequest body) 
        => BanUser(database.GetUserByUsername(username), database, body);
    
    [ApiV3Endpoint("admin/users/uuid/{uuid}/ban", Method.Post), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Bans a user for the specified reason until the given date.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocRequestBody(typeof(ApiPunishUserRequest))]
    public ApiOkResponse BanByUuid(RequestContext context, GameDatabaseContext database, string uuid, ApiPunishUserRequest body) 
        => BanUser(database.GetUserByUuid(uuid), database, body);
    
    [ApiV3Endpoint("admin/users/name/{username}/restrict", Method.Post), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Restricts a user for the specified reason until the given date.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocRequestBody(typeof(ApiPunishUserRequest))]
    public ApiOkResponse RestrictByUsername(RequestContext context, GameDatabaseContext database, string username, ApiPunishUserRequest body) 
        => RestrictUser(database.GetUserByUsername(username), database, body);
    
    [ApiV3Endpoint("admin/users/uuid/{uuid}/restrict", Method.Post), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Restricts a user for the specified reason until the given date.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocRequestBody(typeof(ApiPunishUserRequest))]
    public ApiOkResponse RestrictByUuid(RequestContext context, GameDatabaseContext database, string uuid, ApiPunishUserRequest body) 
        => RestrictUser(database.GetUserByUuid(uuid), database, body);
    
    [ApiV3Endpoint("admin/users/name/{username}/pardon", Method.Post), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Pardons all punishments for the given user.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiOkResponse PardonByUsername(RequestContext context, GameDatabaseContext database, string username) 
        => PardonUser(database.GetUserByUsername(username), database);
    
    [ApiV3Endpoint("admin/users/uuid/{uuid}/pardon", Method.Post), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Pardons all punishments for the given user.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiOkResponse PardonByUuid(RequestContext context, GameDatabaseContext database, string uuid) 
        => PardonUser(database.GetUserByUuid(uuid), database);
}