using AttribDoc.Attributes;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Roles;

namespace Refresh.GameServer.Endpoints.ApiV3.Admin;

public class AdminLevelApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("admin/levels/id/{id}/teamPick", Method.Post), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Marks a level as team picked.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse AddTeamPickToLevel(RequestContext context, GameDatabaseContext database, int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;
        
        database.AddTeamPickToLevel(level);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/levels/id/{id}/removeTeamPick", Method.Post), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Removes a level's team pick status.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse RemoveTeamPickFromLevel(RequestContext context, GameDatabaseContext database, int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;
        
        database.RemoveTeamPickFromLevel(level);
        return new ApiOkResponse();
    }
}