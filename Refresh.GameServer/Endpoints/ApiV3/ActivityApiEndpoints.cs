using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Refresh.GameServer.Database;
using Refresh.GameServer.Documentation.Attributes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class ActivityApiEndpoints : EndpointGroup
{
    // TODO: this api really needs improvement
    // it's recent activity so i'm lazy
    [ApiV3Endpoint("activity"), Authentication(false)]
    [DocUsesPageData, DocSummary("Fetch a list of recent happenings on the server.")]
    [DocQueryParam("timestamp", "A timestamp in unix seconds, used to search backwards.")]
    [DocError(typeof(ApiValidationError), ApiValidationError.NumberParseErrorWhen)]
    public ApiResponse<ApiActivityPageResponse> GetRecentActivity(RequestContext context, GameDatabaseContext database, IDataStore dataStore, GameUser? user, FriendStorageService friendStorageService)
    {
        long timestamp = 0;

        string? tsStr = context.QueryString["timestamp"];
        if (tsStr != null && !long.TryParse(tsStr, out timestamp)) return ApiValidationError.NumberParseError;
        
        (int skip, int count) = context.GetPageData(true);

        ActivityPage page = new(database, new ActivityQueryParameters
        {
            Timestamp = timestamp,
            Count = count,
            Skip = skip,
            User = user,
        }, false, null, friendStorageService);
        return ApiActivityPageResponse.FromOldWithExtraData(page, database, dataStore);
    }
    
    [ApiV3Endpoint("levels/id/{id}/activity"), Authentication(false)]
    [DocUsesPageData, DocSummary("Fetch a list of recent happenings for a particular level")]
    [DocQueryParam("timestamp", "A timestamp in unix seconds, used to search backwards")]
    [DocError(typeof(ApiValidationError), ApiValidationError.NumberParseErrorWhen)]
    [DocError(typeof(ApiNotFoundError), "The level could not be found")]
    public ApiResponse<ApiActivityPageResponse> GetRecentActivityForLevel(RequestContext context, GameDatabaseContext database, IDataStore dataStore, GameUser? user, FriendStorageService friendStorageService,
        [DocSummary("The ID of the level")] int id)
    {
        long timestamp = 0;

        string? tsStr = context.QueryString["timestamp"];
        if (tsStr != null && !long.TryParse(tsStr, out timestamp)) return ApiValidationError.NumberParseError;

        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.Instance;
        
        (int skip, int count) = context.GetPageData(true);
        
        ActivityPage page = new(database, new ActivityQueryParameters
        {
            Timestamp = timestamp,
            Skip = skip,
            Count = count,
            User = user,
        }, false, level, friendStorageService);
        return ApiActivityPageResponse.FromOldWithExtraData(page, database, dataStore);
    }
}