using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Refresh.Database.Query;
using Refresh.Database;
using Refresh.GameServer.Documentation.Attributes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Activity;
using Refresh.Database.Models.Activity;
using Refresh.GameServer.Types.Data;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;
using Refresh.GameServer.Configuration;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class ActivityApiEndpoints : EndpointGroup
{
    // TODO: this api really needs improvement
    // it's recent activity so i'm lazy
    [ApiV3Endpoint("activity"), Authentication(false)]
    [DocUsesPageData, DocSummary("Fetch a list of recent happenings on the server.")]
    [DocQueryParam("timestamp", "A timestamp in unix seconds, used to search backwards.")]
    [DocError(typeof(ApiValidationError), ApiValidationError.NumberParseErrorWhen)]
    public ApiResponse<ApiActivityPageResponse> GetRecentActivity(RequestContext context, GameServerConfig config, GameDatabaseContext database,
        IDataStore dataStore, DataContext dataContext)
    {
        if (!config.PermitShowingOnlineUsers)
            return ApiActivityPageResponse.Empty;

        long timestamp = 0;

        string? tsStr = context.QueryString["timestamp"];
        if (tsStr != null && !long.TryParse(tsStr, out timestamp)) return ApiValidationError.NumberParseError;
        
        (int skip, int count) = context.GetPageData();

        DatabaseActivityPage page = database.GetGlobalRecentActivityPage(new ActivityQueryParameters
        {
            Timestamp = timestamp,
            Count = count,
            Skip = skip,
        });
        return ApiActivityPageResponse.FromOld(page, dataContext);
    }
    
    [ApiV3Endpoint("levels/id/{id}/activity"), Authentication(false)]
    [DocUsesPageData, DocSummary("Fetch a list of recent happenings for a particular level")]
    [DocQueryParam("timestamp", "A timestamp in unix seconds, used to search backwards")]
    [DocError(typeof(ApiValidationError), ApiValidationError.NumberParseErrorWhen)]
    [DocError(typeof(ApiNotFoundError), "The level could not be found")]
    public ApiResponse<ApiActivityPageResponse> GetRecentActivityForLevel(RequestContext context,
        GameServerConfig config, GameDatabaseContext database, IDataStore dataStore, GameUser? user,
        [DocSummary("The ID of the level")] int id, DataContext dataContext)
    {
        if (!config.PermitShowingOnlineUsers)
            return ApiActivityPageResponse.Empty;

        long timestamp = 0;

        string? tsStr = context.QueryString["timestamp"];
        if (tsStr != null && !long.TryParse(tsStr, out timestamp)) return ApiValidationError.NumberParseError;

        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.Instance;
        
        (int skip, int count) = context.GetPageData();

        DatabaseActivityPage page = database.GetRecentActivityForLevel(level, new ActivityQueryParameters
        {
            Timestamp = timestamp,
            Skip = skip,
            Count = count,
            User = user,
        });
        return ApiActivityPageResponse.FromOld(page, dataContext);
    }
    
    [ApiV3Endpoint("users/uuid/{uuid}/activity"), Authentication(false)]
    [DocUsesPageData, DocSummary("Fetch a list of recent happenings for a particular user")]
    [DocQueryParam("timestamp", "A timestamp in unix seconds, used to search backwards")]
    [DocError(typeof(ApiValidationError), ApiValidationError.NumberParseErrorWhen)]
    [DocError(typeof(ApiNotFoundError), "The user could not be found")]
    public ApiResponse<ApiActivityPageResponse> GetRecentActivityForUserUuid(RequestContext context,
        GameServerConfig config, GameDatabaseContext database, IDataStore dataStore,
        [DocSummary("The UUID of the user")] string uuid, DataContext dataContext)
    {
        if (!config.PermitShowingOnlineUsers)
            return ApiActivityPageResponse.Empty;

        long timestamp = 0;

        string? tsStr = context.QueryString["timestamp"];
        if (tsStr != null && !long.TryParse(tsStr, out timestamp)) return ApiValidationError.NumberParseError;
        
        GameUser? user = database.GetUserByUuid(uuid);
        if (user == null) return ApiNotFoundError.Instance;
        
        (int skip, int count) = context.GetPageData();

        DatabaseActivityPage page = database.GetRecentActivityFromUser(new ActivityQueryParameters
        {
            Timestamp = timestamp,
            Skip = skip,
            Count = count,
            User = user,
        });
        return ApiActivityPageResponse.FromOld(page, dataContext);
    }
    
    [ApiV3Endpoint("users/name/{username}/activity"), Authentication(false)]
    [DocUsesPageData, DocSummary("Fetch a list of recent happenings for a particular user")]
    [DocQueryParam("timestamp", "A timestamp in unix seconds, used to search backwards")]
    [DocError(typeof(ApiValidationError), ApiValidationError.NumberParseErrorWhen)]
    [DocError(typeof(ApiNotFoundError), "The user could not be found")]
    public ApiResponse<ApiActivityPageResponse> GetRecentActivityForUserUsername(RequestContext context,
        GameServerConfig config, GameDatabaseContext database, IDataStore dataStore,
        [DocSummary("The username of the user")] string username, DataContext dataContext)
    {
        if (!config.PermitShowingOnlineUsers)
            return ApiActivityPageResponse.Empty;

        long timestamp = 0;

        string? tsStr = context.QueryString["timestamp"];
        if (tsStr != null && !long.TryParse(tsStr, out timestamp)) return ApiValidationError.NumberParseError;
        
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return ApiNotFoundError.Instance;
        
        (int skip, int count) = context.GetPageData();

        DatabaseActivityPage page = database.GetRecentActivityFromUser(new ActivityQueryParameters
        {
            Timestamp = timestamp,
            Skip = skip,
            Count = count,
            User = user,
        });
        return ApiActivityPageResponse.FromOld(page, dataContext);
    }
}