using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Bunkum.Protocols.Http;
using Refresh.Common.Constants;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Documentation.Attributes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Levels;
using Refresh.Interfaces.APIv3.Extensions;

namespace Refresh.Interfaces.APIv3.Endpoints;

public class ReviewApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("levels/id/{id}/reviews"), Authentication(false)]
    [DocUsesPageData, DocSummary("Gets a list of the reviews posted to a level.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiListResponse<ApiGameReviewResponse> GetReviewsForLevel(RequestContext context,
        GameDatabaseContext database, IDataStore dataStore,
        [DocSummary("The ID of the level")] int id, DataContext dataContext)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;
        
        (int skip, int count) = context.GetPageData();
        
        DatabaseList<GameReview> reviews = database.GetReviewsForLevel(level, count, skip);
        DatabaseList<ApiGameReviewResponse> ret = DatabaseListExtensions.FromOldList<ApiGameReviewResponse, GameReview>(reviews, dataContext);

        return ret;
    }

    [ApiV3Endpoint("users/{userIdType}/{userId}/reviews"), Authentication(false)]
    [DocUsesPageData, DocSummary("Gets a list of the reviews posted by a user.")]
    [DocError(typeof(ApiValidationError), ApiValidationError.BadUserLookupIdTypeWhen)]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiListResponse<ApiGameReviewResponse> GetReviewsByUser(RequestContext context,
        GameDatabaseContext database, IDataStore dataStore, DataContext dataContext,
        [DocSummary("The type of identifier used to look up the user. Can be either 'uuid' or 'username'.")] string userIdType,
        [DocSummary("The UUID or username of the user, depending on the specified ID type.")] string userId)
    {
        GameUser? user;
        switch (userIdType)
        {
            case "uuid":
                user = database.GetUserByUuid(userId);
                break;
            case "username":
                user = database.GetUserByUsername(userId);
                break;
            default:
                return ApiValidationError.BadUserLookupIdType;
        };

        if (user == null) return ApiNotFoundError.UserMissingError;
        
        (int skip, int count) = context.GetPageData();
        
        DatabaseList<GameReview> reviews = database.GetReviewsByUser(user, count, skip);
        DatabaseList<ApiGameReviewResponse> ret = DatabaseListExtensions.FromOldList<ApiGameReviewResponse, GameReview>(reviews, dataContext);

        return ret;
    }

    [ApiV3Endpoint("reviews/id/{id}"), Authentication(false)]
    [DocSummary("Gets a review by ID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.ReviewMissingErrorWhen)]
    public ApiResponse<ApiGameReviewResponse> GetReviewById(RequestContext context,
        GameDatabaseContext database, IDataStore dataStore,
        [DocSummary("The ID of the review")] int id, DataContext dataContext)
    {
        GameReview? review = database.GetReviewById(id);
        if (review == null) return ApiNotFoundError.ReviewMissingError;
        
        return ApiGameReviewResponse.FromOld(review, dataContext);
    }

    /// <returns>
    /// The validated list of labels. Labels which are duplicates or invalid will be removed.
    /// Also, the list will be trimmed if there are too many labels.
    /// </returns>
    private List<Label> ValidateLabels(List<Label> input)
        => input
            .Distinct()
            .Where(l => Enum.IsDefined(l))
            .Take(UgcLimits.MaximumLabels)
            .ToList();

    [ApiV3Endpoint("levels/id/{id}/reviews", HttpMethods.Post)]
    [DocSummary("Posts a review to the specified level. Updates the user's current review if they've already posted one.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.DontReviewOwnLevelWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.DontReviewLevelBeforePlayingWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.RatingParseErrorWhen)]
    public ApiResponse<ApiGameReviewResponse> PostReviewToLevel(RequestContext context,
        GameDatabaseContext database, IDataStore dataStore, GameUser user,
        [DocSummary("The ID of the level")] int id, ApiSubmitReviewRequest body, DataContext dataContext)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        // Level publishers shouldn't be able to review their own level
        if (user.UserId == level.Publisher?.UserId)
            return ApiValidationError.DontReviewOwnLevel;
        
        // A user has to play a level before they may review it
        if (!database.HasUserPlayedLevel(level, user))
            return ApiValidationError.DontReviewLevelBeforePlaying;
        
        // Validate labels
        if (body.Labels != null)
        {
            body.Labels = this.ValidateLabels(body.Labels);
        }
        
        if (body.LevelRating != null)
        {
            if (!Enum.IsDefined(typeof(RatingType), body.LevelRating)) 
                return ApiValidationError.RatingParseError;
            
            database.RateLevel(level, user, body.LevelRating.Value);
        }

        if (body.Content != null && body.Content.Length > UgcLimits.CommentLimit)
        {
            body.Content = body.Content[..UgcLimits.CommentLimit];
        }

        GameReview review = database.AddReviewToLevel(body, level, user);
        return ApiGameReviewResponse.FromOld(review, dataContext);
    }

    [ApiV3Endpoint("reviews/id/{id}", HttpMethods.Patch)]
    [DocSummary("Updates a review by ID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.ReviewMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.NoReviewEditPermissionErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.RatingParseErrorWhen)]
    public ApiResponse<ApiGameReviewResponse> UpdateReviewById(RequestContext context,
        GameDatabaseContext database, IDataStore dataStore, GameUser user,
        [DocSummary("The ID of the review")] int id, ApiSubmitReviewRequest body, DataContext dataContext)
    {
        GameReview? review = database.GetReviewById(id);
        if (review == null) return ApiNotFoundError.ReviewMissingError;

        // Don't allow users to update other users' reviews
        if (review.PublisherUserId != user.UserId) 
            return ApiValidationError.NoReviewEditPermissionError;

        // Validate labels
        if (body.Labels != null)
        {
            body.Labels = this.ValidateLabels(body.Labels);
        }

        if (body.LevelRating != null)
        {
            if (!Enum.IsDefined(typeof(RatingType), body.LevelRating)) 
                return ApiValidationError.RatingParseError;
            
            database.RateLevel(review.Level, user, body.LevelRating.Value);
        }

        if (body.Content != null && body.Content.Length > UgcLimits.CommentLimit)
        {
            body.Content = body.Content[..UgcLimits.CommentLimit];
        }
        
        review = database.UpdateReview(body, review);
        return ApiGameReviewResponse.FromOld(review, dataContext);
    }

    [ApiV3Endpoint("reviews/id/{id}", HttpMethods.Delete)]
    [DocSummary("Deletes a review by ID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.ReviewMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.NoReviewDeletionPermissionErrorWhen)]
    public ApiOkResponse DeleteReviewById(RequestContext context,
        GameDatabaseContext database, IDataStore dataStore, GameUser user,
        [DocSummary("The ID of the review")] int id, DataContext dataContext)
    {
        GameReview? review = database.GetReviewById(id);
        if (review == null) return ApiNotFoundError.ReviewMissingError;

        // Only allow this review to be deleted by either its publisher or its level's publisher
        if (user.UserId != review.PublisherUserId && user.UserId != review.Level.Publisher?.UserId)
            return ApiValidationError.NoReviewDeletionPermissionError;

        database.DeleteReview(review);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("reviews/id/{id}/rate/{rawRating}", HttpMethods.Post)]
    [DocSummary("Rates a review by ID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.ReviewMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.DontRateOwnContentWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.RatingParseErrorWhen)]
    public ApiOkResponse RateReviewById(RequestContext context,
        GameDatabaseContext database, IDataStore dataStore, GameUser user,
        [DocSummary("The ID of the review")] int id, DataContext dataContext,
        [DocSummary("The user's new rating. -1 = dislike, 0 = neutral, 1 = like.")] string rawRating)
    {
        GameReview? review = database.GetReviewById(id);
        if (review == null) return ApiNotFoundError.ReviewMissingError;

        // Review publishers shouldn't be able to rate their own review
        if (user.UserId == review.PublisherUserId) 
            return ApiValidationError.DontRateOwnContent;

        // See CommentApiEndpoints.RateLevelComment()
        if (!sbyte.TryParse(rawRating, out sbyte rating) || !Enum.IsDefined(typeof(RatingType), rating))
            return ApiValidationError.RatingParseError;

        dataContext.Database.RateReview(review, (RatingType)rating, user);
        return new ApiOkResponse();
    }
}