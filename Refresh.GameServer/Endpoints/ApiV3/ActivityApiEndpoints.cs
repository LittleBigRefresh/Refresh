using AttribDoc.Attributes;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class ActivityApiEndpoints : EndpointGroup
{
    // TODO: this api really needs improvement
    // it's recent activity so i'm lazy
    [ApiV3Endpoint("activity"), Authentication(false)]
    [DocSummary("Fetch a list of recent happenings on the server.")]
    [DocQueryParam("timestamp", "A timestamp in unix seconds, used to search backwards.")]
    [DocError(typeof(ApiValidationError), ApiValidationError.NumberParseErrorWhen)]
    public ApiResponse<ApiActivityPageResponse> GetRecentActivity(RequestContext context, GameDatabaseContext database)
    {
        long timestamp = 0;

        string? tsStr = context.QueryString["timestamp"];
        if (tsStr != null && !long.TryParse(tsStr, out timestamp)) return ApiValidationError.NumberParseError;

        ActivityPage page = new(database, generateGroups: false, timestamp: timestamp);
        return ApiActivityPageResponse.FromOld(page);
    }
    
    
    [ApiV3Endpoint("levels/id/{id}/activity"), Authentication(false)]
    [DocSummary("Fetch a list of recent happenings for a particular level")]
    [DocQueryParam("timestamp", "A timestamp in unix seconds, used to search backwards")]
    [DocError(typeof(ApiValidationError), ApiValidationError.NumberParseErrorWhen)]
    [DocError(typeof(ApiNotFoundError), "The level could not be found")]
    public ApiResponse<ApiActivityPageResponse> GetRecentActivityForLevel(RequestContext context, GameDatabaseContext database,
        [DocSummary("The ID of the level")] int id)
    {
        long timestamp = 0;

        string? tsStr = context.QueryString["timestamp"];
        if (tsStr != null && !long.TryParse(tsStr, out timestamp)) return ApiValidationError.NumberParseError;

        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.Instance;
        
        ActivityPage page = new(database, generateGroups: false, timestamp: timestamp, level: level);
        return ApiActivityPageResponse.FromOld(page);
    }
}