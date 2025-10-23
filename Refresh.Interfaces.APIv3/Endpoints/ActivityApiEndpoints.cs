using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Refresh.Core.Configuration;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Activity;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;
using Refresh.Interfaces.APIv3.Documentation.Attributes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Activity;

namespace Refresh.Interfaces.APIv3.Endpoints;

public class ActivityApiEndpoints : EndpointGroup
{
    public (ActivityQueryParameters?, ApiError?) ParseParameters(RequestContext context, GameUser? user)
    {
        long timestamp = 0;
        
        bool excludeMyLevels = bool.Parse(context.QueryString["excludeMyLevels"] ?? "false");
        bool excludeFriends = bool.Parse(context.QueryString["excludeFriends"] ?? "false");
        bool excludeFavouriteUsers = bool.Parse(context.QueryString["excludeFavouriteUsers"] ?? "false");
        bool excludeMyself = bool.Parse(context.QueryString["excludeMyself"] ?? "false");

        // Preserve original API behaviour to keep refresh-web and other clients from breaking
        bool includeActivity = bool.Parse(context.QueryString["includeActivity"] ?? "true");
        bool includeDeletedActivity = bool.Parse(context.QueryString["includeDeletedActivity"] ?? "false");
        bool IncludeModeration = bool.Parse(context.QueryString["IncludeModeration"] ?? "false");

        string? tsStr = context.QueryString["timestamp"];

        if (tsStr != null && !long.TryParse(tsStr, out timestamp))
            return (null, ApiValidationError.NumberParseError);
        
        (int skip, int count) = context.GetPageData();

        ActivityQueryParameters ret = new()
        {
            Timestamp = timestamp,
            Count = count,
            Skip = skip,
            User = user,
            ExcludeFriends = excludeFriends,
            ExcludeMyLevels = excludeMyLevels,
            ExcludeFavouriteUsers = excludeFavouriteUsers,
            ExcludeMyself = excludeMyself,
            IncludeActivity = includeActivity,
            IncludeDeletedActivity = includeDeletedActivity,
            IncludeModeration = IncludeModeration,
        };

        return (ret, null);
    }

    // TODO: this api really needs improvement
    // it's recent activity so i'm lazy
    [ApiV3Endpoint("activity"), Authentication(false)]
    [DocUsesPageData, DocSummary("Fetch a list of recent happenings on the server.")]
    [DocQueryParam("timestamp", "A timestamp in unix seconds, used to search backwards.")]
    [DocError(typeof(ApiValidationError), ApiValidationError.NumberParseErrorWhen)]
    public ApiResponse<ApiActivityPageResponse> GetRecentActivity(RequestContext context, GameServerConfig config, GameDatabaseContext database,
        GameUser? user, IDataStore dataStore, DataContext dataContext)
    {
        if (!config.PermitShowingOnlineUsers)
            return ApiActivityPageResponse.Empty;

        (ActivityQueryParameters? parameters, ApiError? error) = this.ParseParameters(context, user);
        if (error != null)
        {
            return error;
        }
        else if (parameters == null)
        {
            return ApiInternalError.ParamParsingFailed;
        }

        DatabaseActivityPage page = database.GetGlobalRecentActivityPage(parameters.Value);
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

        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.Instance;

        (ActivityQueryParameters? parameters, ApiError? error) = this.ParseParameters(context, user);
        if (error != null)
        {
            return error;
        }
        else if (parameters == null)
        {
            return ApiInternalError.ParamParsingFailed;
        }

        DatabaseActivityPage page = database.GetRecentActivityForLevel(level, parameters.Value);
        return ApiActivityPageResponse.FromOld(page, dataContext);
    }
    
    [ApiV3Endpoint("users/uuid/{uuid}/activity"), Authentication(false)]
    [DocUsesPageData, DocSummary("Fetch a list of recent happenings for a particular user")]
    [DocQueryParam("timestamp", "A timestamp in unix seconds, used to search backwards")]
    [DocError(typeof(ApiValidationError), ApiValidationError.NumberParseErrorWhen)]
    [DocError(typeof(ApiNotFoundError), "The user could not be found")]
    public ApiResponse<ApiActivityPageResponse> GetRecentActivityForUserUuid(RequestContext context,
        GameServerConfig config, GameDatabaseContext database, IDataStore dataStore,
        [DocSummary("The UUID of the user")] string uuid, GameUser? user, DataContext dataContext)
    {
        if (!config.PermitShowingOnlineUsers)
            return ApiActivityPageResponse.Empty;
        
        GameUser? targetUser = database.GetUserByUuid(uuid);
        if (targetUser == null) return ApiNotFoundError.Instance;

        (ActivityQueryParameters? parameters, ApiError? error) = this.ParseParameters(context, user);
        if (error != null)
        {
            return error;
        }
        else if (parameters == null)
        {
            return ApiInternalError.ParamParsingFailed;
        }

        DatabaseActivityPage page = database.GetRecentActivityFromUser(targetUser, parameters.Value);
        return ApiActivityPageResponse.FromOld(page, dataContext);
    }
    
    [ApiV3Endpoint("users/name/{username}/activity"), Authentication(false)]
    [DocUsesPageData, DocSummary("Fetch a list of recent happenings for a particular user")]
    [DocQueryParam("timestamp", "A timestamp in unix seconds, used to search backwards")]
    [DocError(typeof(ApiValidationError), ApiValidationError.NumberParseErrorWhen)]
    [DocError(typeof(ApiNotFoundError), "The user could not be found")]
    public ApiResponse<ApiActivityPageResponse> GetRecentActivityForUserUsername(RequestContext context,
        GameServerConfig config, GameDatabaseContext database, IDataStore dataStore,
        [DocSummary("The username of the user")] string username, GameUser? user, DataContext dataContext)
    {
        if (!config.PermitShowingOnlineUsers)
            return ApiActivityPageResponse.Empty;
        
        GameUser? targetUser = database.GetUserByUsername(username);
        if (targetUser == null) return ApiNotFoundError.Instance;

        (ActivityQueryParameters? parameters, ApiError? error) = this.ParseParameters(context, user);
        if (error != null)
        {
            return error;
        }
        else if (parameters == null)
        {
            return ApiInternalError.ParamParsingFailed;
        }

        DatabaseActivityPage page = database.GetRecentActivityFromUser(targetUser, parameters.Value);
        return ApiActivityPageResponse.FromOld(page, dataContext);
    }
}