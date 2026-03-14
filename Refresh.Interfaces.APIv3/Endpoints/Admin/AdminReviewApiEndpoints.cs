using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Http;
using Refresh.Core.Authentication.Permission;
using Refresh.Database;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Documentation.Descriptions;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;

namespace Refresh.Interfaces.APIv3.Endpoints.Admin;

public class AdminReviewApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("admin/reviews/id/{reviewId}", HttpMethods.Delete), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Deletes a specific review by ID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.ReviewMissingErrorWhen)]
    public ApiOkResponse DeleteReviewById(RequestContext context, GameDatabaseContext database,
        [DocSummary("The ID of the review to delete")] int reviewId)
    {
        GameReview? review = database.GetReviewById(reviewId);
        
        if (review == null) return ApiNotFoundError.ReviewMissingError;
        
        database.DeleteReview(review);
        return new ApiOkResponse();
    }
    
        [ApiV3Endpoint("admin/comments/profile/id/{commentId}", HttpMethods.Delete), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Deletes a specific profile comment by ID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.CommentMissingErrorWhen)]
    public ApiOkResponse DeleteProfileCommentById(RequestContext context, GameDatabaseContext database,
        [DocSummary("The ID of the profile comment to delete")] int commentId)
    {
        GameProfileComment? comment = database.GetProfileCommentById(commentId);
        
        if (comment == null) return ApiNotFoundError.CommentMissingError;
        
        database.DeleteProfileComment(comment);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/comments/level/id/{commentId}", HttpMethods.Delete), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Deletes a specific level comment by ID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.CommentMissingErrorWhen)]
    public ApiOkResponse DeleteLevelCommentById(RequestContext context, GameDatabaseContext database,
        [DocSummary("The ID of the level comment to delete")] int commentId)
    {
        GameLevelComment? comment = database.GetLevelCommentById(commentId);
        
        if (comment == null) return ApiNotFoundError.CommentMissingError;
        
        database.DeleteLevelComment(comment);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/users/{idType}/{id}/comments/profile", HttpMethods.Delete), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Deletes all profile comments posted by a user. Gets user by their UUID or username.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiOkResponse DeleteProfileCommentsByUser(RequestContext context, GameDatabaseContext database,
        [DocSummary(SharedParamDescriptions.UserIdParam)] string id, 
        [DocSummary(SharedParamDescriptions.UserIdTypeParam)] string idType)
    {
        GameUser? user = database.GetUserByIdAndType(idType, id);
        if (user == null) return ApiNotFoundError.UserMissingError;
        
        database.DeleteProfileCommentsPostedByUser(user);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/users/{idType}/{id}/comments/level", HttpMethods.Delete), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Deletes all level comments posted by a user. Gets user by their UUID or username.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiOkResponse DeleteLevelCommentsByUser(RequestContext context, GameDatabaseContext database,
        [DocSummary(SharedParamDescriptions.UserIdParam)] string id, 
        [DocSummary(SharedParamDescriptions.UserIdTypeParam)] string idType)
    {
        GameUser? user = database.GetUserByIdAndType(idType, id);
        if (user == null) return ApiNotFoundError.UserMissingError;

        database.DeleteLevelCommentsPostedByUser(user);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/users/{idType}/{id}/reviews", HttpMethods.Delete), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Deletes all reviews posted by a user. Gets user by their UUID or username.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiOkResponse DeleteReviewsPostedByUser(RequestContext context, GameDatabaseContext database,
        [DocSummary(SharedParamDescriptions.UserIdParam)] string id, 
        [DocSummary(SharedParamDescriptions.UserIdTypeParam)] string idType)
    {
        GameUser? user = database.GetUserByIdAndType(idType, id);
        if (user == null) return ApiNotFoundError.UserMissingError;
        
        database.DeleteReviewsPostedByUser(user);
        return new ApiOkResponse();
    }
}