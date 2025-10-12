using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Bunkum.Protocols.Http;
using Refresh.Common.Verification;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Services;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Pins;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Levels;

namespace Refresh.Interfaces.APIv3.Endpoints;

public class LevelApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("levels/id/{id}"), Authentication(false)]
    [DocSummary("Gets an individual level by a numerical ID")]
    [DocError(typeof(ApiNotFoundError), "The level cannot be found")]
    public ApiResponse<ApiGameLevelResponse> GetLevelById(RequestContext context, GameDatabaseContext database,
        IDataStore dataStore,
        [DocSummary("The ID of the level")] int id, DataContext dataContext)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;
        
        return ApiGameLevelResponse.FromOld(level, dataContext);
    }
    
    [ApiV3Endpoint("levels/hash/{hash}"), Authentication(false)]
    [DocSummary("Gets an individual level by the level's RootResource hash")]
    [DocError(typeof(ApiNotFoundError), "The level cannot be found")]
    public ApiResponse<ApiGameLevelResponse> GetLevelByRootResource(RequestContext context, GameDatabaseContext database,
        IDataStore dataStore,
        [DocSummary("The RootResource hash of the level")] string hash, DataContext dataContext)
    {
        GameLevel? level = database.GetLevelByRootResource(hash);
        if (level == null) return ApiNotFoundError.LevelMissingError;
        
        return ApiGameLevelResponse.FromOld(level, dataContext);
    }
    
    [ApiV3Endpoint("levels/id/{id}", HttpMethods.Patch)]
    [DocSummary("Edits a level by the level's numerical ID")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    [DocError(typeof(ApiAuthenticationError), ApiAuthenticationError.NoPermissionsForObjectWhen)]
    public ApiResponse<ApiGameLevelResponse> EditLevelById(RequestContext context,
        [DocSummary("The ID of the level")] int id, ApiEditLevelRequest body, DataContext dataContext)
    {
        GameLevel? level = dataContext.Database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;
        
        if (level.Publisher?.UserId != dataContext.User!.UserId) 
            return ApiAuthenticationError.NoPermissionsForObject;

        if (body.IconHash != null && body.IconHash.StartsWith('g') &&
            !dataContext.GuidChecker.IsTextureGuid(level.GameVersion, long.Parse(body.IconHash)))
            return ApiValidationError.InvalidTextureGuidError;
        
        level = dataContext.Database.UpdateLevel(body, level, dataContext.User);

        return ApiGameLevelResponse.FromOld(level, dataContext);
    }

    [ApiV3Endpoint("levels/id/{id}", HttpMethods.Delete)]
    [DocSummary("Deletes a level by the level's numerical ID")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    [DocError(typeof(ApiAuthenticationError), ApiAuthenticationError.NoPermissionsForObjectWhen)]
    public ApiOkResponse DeleteLevelById(RequestContext context, GameDatabaseContext database, GameUser user,
        [DocSummary("The ID of the level")] int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        if (level.Publisher?.UserId != user.UserId) 
            return ApiAuthenticationError.NoPermissionsForObject;

        database.DeleteLevel(level);

        return new ApiOkResponse();
    }

    [ApiV3Endpoint("levels/id/{id}/setAsOverride", HttpMethods.Post)]
    [DocSummary("Marks the level to show in the next slot list gotten from the game")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse SetLevelAsOverrideById(RequestContext context, 
        GameDatabaseContext database, 
        GameUser user, 
        PlayNowService overrideService,
        [DocSummary("The ID of the level")] int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        // TODO: return whether or not the presence server was used
        overrideService.PlayNowLevel(user, level);
        
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("levels/hash/{hash}/setAsOverride", HttpMethods.Post)]
    [DocSummary("Marks the level hash to show in the next slot list gotten from the game")]
    [DocError(typeof(ApiValidationError), ApiValidationError.HashInvalidErrorWhen)]
    public ApiOkResponse SetLevelAsOverrideByHash(RequestContext context, GameDatabaseContext database, GameUser user,
        PlayNowService service, PresenceService presenceService, [DocSummary("The hash of level root resource")] string hash)
    {
        if (!CommonPatterns.Sha1Regex().IsMatch(hash)) 
            return ApiValidationError.HashInvalidError;

        // TODO: return whether presence/hash play now was used
        service.PlayNowHash(user, hash);
        
        return new ApiOkResponse();
    }


    [ApiV3Endpoint("levels/id/{id}/relations"), MinimumRole(GameUserRole.Restricted)]
    [DocSummary("Gets your relations to a level by it's ID")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiResponse<ApiGameLevelRelationsResponse> GetLevelRelationsOfUser(RequestContext context, GameDatabaseContext database, GameUser user,
        [DocSummary("The ID of the level")] int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        return new ApiGameLevelRelationsResponse
        {
            IsHearted = database.IsLevelFavouritedByUser(level, user),
            IsQueued = database.IsLevelQueuedByUser(level, user),
            MyPlaysCount = database.GetTotalPlaysForLevelByUser(level, user)
        };
    }

    [ApiV3Endpoint("levels/id/{id}/heart", HttpMethods.Post)]
    [DocSummary("Adds a specific level by it's ID to your hearted levels")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse FavouriteLevel(RequestContext context, GameDatabaseContext database, GameUser user,
        [DocSummary("The ID of the level")] int id) 
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        database.FavouriteLevel(level, user);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("levels/id/{id}/unheart", HttpMethods.Post)]
    [DocSummary("Removes a specific level by it's ID from your hearted levels")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse UnheartLevel(RequestContext context, GameDatabaseContext database, GameUser user,
        [DocSummary("The ID of the level")] int id) 
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        database.UnfavouriteLevel(level, user);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("levels/id/{id}/queue", HttpMethods.Post)]
    [DocSummary("Adds a specific level by it's ID to your queue")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse QueueLevel(RequestContext context, GameDatabaseContext database, GameUser user,
        [DocSummary("The ID of the level")] int id) 
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        database.QueueLevel(level, user);

        // Update pin progress for queueing a level through the API
        database.IncrementUserPinProgress((long)ServerPins.QueueLevelOnWebsite, 1, user);

        return new ApiOkResponse();
    }

    [ApiV3Endpoint("levels/id/{id}/dequeue", HttpMethods.Post)]
    [DocSummary("Removes a specific level by it's ID from your queue")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse DequeueLevel(RequestContext context, GameDatabaseContext database, GameUser user,
        [DocSummary("The ID of the level")] int id) 
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        database.DequeueLevel(level, user);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("levels/queued/clear", HttpMethods.Post)]
    [DocSummary("Clears your level queue")]
    public ApiOkResponse ClearQueuedLevels(RequestContext context, GameDatabaseContext database,
        IDataStore dataStore, GameUser user, DataContext dataContext) 
    {
        database.ClearQueue(user);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("levels/id/{id}/rate/{rawRating}", HttpMethods.Post)]
    [DocSummary("Rates a level by ID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse RateLevelById(RequestContext context,
        GameDatabaseContext database, IDataStore dataStore, GameUser user,
        [DocSummary("The ID of the level")] int id, DataContext dataContext,
        [DocSummary("The user's new rating. -1 = dislike, 0 = neutral, 1 = like.")] string rawRating)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        // Level publishers shouldn't be able to rate their own level
        if (user == level.Publisher) // TODO: compare UUIDs
            return ApiValidationError.DontRateOwnContent;

        // See CommentApiEndpoints.RateLevelComment()
        if (!sbyte.TryParse(rawRating, out sbyte rating) || !Enum.IsDefined(typeof(RatingType), rating))
            return ApiValidationError.RatingParseError;

        dataContext.Database.RateLevel(level, user, (RatingType)rating);
        return new ApiOkResponse();
    }
}