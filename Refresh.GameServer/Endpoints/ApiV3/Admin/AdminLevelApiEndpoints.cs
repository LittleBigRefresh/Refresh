using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3.Admin;

public class AdminLevelApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("admin/levels/id/{id}/teamPick", HttpMethods.Post), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Marks a level as team picked.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse AddTeamPickToLevel(RequestContext context, GameDatabaseContext database, GameUser user, int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;
        
        database.AddTeamPickToLevel(level);
        database.CreateLevelTeamPickEvent(user, level);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/levels/id/{id}/removeTeamPick", HttpMethods.Post), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Removes a level's team pick status.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse RemoveTeamPickFromLevel(RequestContext context, GameDatabaseContext database, int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;
        
        database.RemoveTeamPickFromLevel(level);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/levels/id/{id}", HttpMethods.Patch), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Updates a level.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    [DocError(typeof(ApiAuthenticationError), ApiAuthenticationError.NoPermissionsForObjectWhen)]
    public ApiResponse<ApiGameLevelResponse> EditLevelById(RequestContext context, GameDatabaseContext database,
        GameUser user,
        [DocSummary("The ID of the level")] int id, ApiAdminEditLevelRequest body, DataContext dataContext)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        level = database.UpdateLevel(body, level);

        return ApiGameLevelResponse.FromOld(level, dataContext);
    }
    
    [ApiV3Endpoint("admin/levels/id/{id}", HttpMethods.Delete), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Deletes a level.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse DeleteLevel(RequestContext context, GameDatabaseContext database, int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;
        
        database.DeleteLevel(level);
        return new ApiOkResponse();
    }
}