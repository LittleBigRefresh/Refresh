using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Http;
using Refresh.Common.Constants;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Documentation.Attributes;
using Refresh.Interfaces.APIv3.Documentation.Descriptions;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Comments;
using Refresh.Interfaces.APIv3.Extensions;

namespace Refresh.Interfaces.APIv3.Endpoints;

public class CommentApiEndpoints : EndpointGroup
{
    #region Profile
    [ApiV3Endpoint("users/{idType}/{id}/comments"), Authentication(false)]
    [DocSummary("Gets comments posted under the specified user's profile.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocUsesPageData]
    public ApiListResponse<ApiProfileCommentResponse> GetCommentsOnProfile(RequestContext context, DataContext dataContext,
        [DocSummary(SharedParamDescriptions.UserIdParam)] string id, 
        [DocSummary(SharedParamDescriptions.UserIdTypeParam)] string idType)
    {
        GameUser? profile = dataContext.Database.GetUserByIdAndType(idType, id);
        if (profile == null) return ApiNotFoundError.UserMissingError;

        (int skip, int count) = context.GetPageData();

        DatabaseList<GameProfileComment> comments = dataContext.Database.GetProfileComments(profile, count, skip);
        return DatabaseListExtensions.FromOldList<ApiProfileCommentResponse, GameProfileComment>(comments, dataContext);
    }

    [ApiV3Endpoint("users/{idType}/{id}/comments", HttpMethods.Post)]
    [DocSummary("Posts the given comment under the specified user's profile.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiResponse<ApiProfileCommentResponse> PostCommentOnProfile(RequestContext context,
        DataContext dataContext, GameUser user, ApiCommentPostRequest body,
        [DocSummary(SharedParamDescriptions.UserIdParam)] string id, 
        [DocSummary(SharedParamDescriptions.UserIdTypeParam)] string idType)
    {
        GameUser? profile = dataContext.Database.GetUserByIdAndType(idType, id);
        if (profile == null) return ApiNotFoundError.UserMissingError;

        // Trim content
        if (body.Content.Length > UgcLimits.CommentLimit) 
            body.Content = body.Content[..UgcLimits.CommentLimit];

        GameProfileComment comment = dataContext.Database.PostCommentToProfile(profile, user, body.Content);
        return ApiProfileCommentResponse.FromOld(comment, dataContext);
    }

    [ApiV3Endpoint("profileComments/id/{id}"), Authentication(false)]
    [DocSummary("Gets the profile comment specified by its ID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.CommentMissingErrorWhen)]
    public ApiResponse<ApiProfileCommentResponse> GetProfileComment(RequestContext context, DataContext dataContext, int id)
    {
        GameProfileComment? comment = dataContext.Database.GetProfileCommentById(id);
        if (comment == null) return ApiNotFoundError.CommentMissingError;

        return ApiProfileCommentResponse.FromOld(comment, dataContext);
    }
    
    [ApiV3Endpoint("profileComments/id/{id}", HttpMethods.Delete)]
    [DocSummary("Deletes the profile comment specified by its ID. Fails if the user is not the comment poster or the profile owner.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.CommentMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.NoCommentDeletionPermissionErrorWhen)]
    public ApiOkResponse DeleteProfileComment(RequestContext context, DataContext dataContext, GameUser user, int id)
    {
        GameProfileComment? comment = dataContext.Database.GetProfileCommentById(id);
        if (comment == null) return ApiNotFoundError.CommentMissingError;

        if (user != comment.Author && user != comment.Profile) return ApiValidationError.NoCommentDeletionPermissionError;

        dataContext.Database.DeleteProfileComment(comment);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("profileComments/id/{id}/rate/{rawRating}", HttpMethods.Post)]
    [DocSummary("Rates the profile comment specified by its ID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.CommentMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.RatingParseErrorWhen)]
    public ApiOkResponse RateProfileComment(RequestContext context, DataContext dataContext, GameUser user, int id, 
        [DocSummary("The user's new rating for the comment. -1 = dislike, 0 = neutral, 1 = like.")] string rawRating)
    {
        GameProfileComment? comment = dataContext.Database.GetProfileCommentById(id);
        if (comment == null) return ApiNotFoundError.CommentMissingError;

        // rawRating is string and not sbyte or integer because passing any out of range value will make Bunkum 
        // set rawRating to 0 instead, which we would here wrongly take as a neutral rating instead of an invalid value.
        if (!sbyte.TryParse(rawRating, out sbyte rating) || !Enum.IsDefined(typeof(RatingType), rating))
            return ApiValidationError.RatingParseError;

        dataContext.Database.RateProfileComment(user, comment, (RatingType)rating);
        return new ApiOkResponse();
    }
    #endregion

    #region Level
    [ApiV3Endpoint("levels/id/{id}/comments"), Authentication(false)]
    [DocSummary("Gets comments posted under the specified level.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    [DocUsesPageData]
    public ApiListResponse<ApiLevelCommentResponse> GetCommentsOnLevel(RequestContext context, DataContext dataContext, int id)
    {
        GameLevel? level = dataContext.Database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        (int skip, int count) = context.GetPageData();

        DatabaseList<GameLevelComment> comments = dataContext.Database.GetLevelComments(level, count, skip);
        return DatabaseListExtensions.FromOldList<ApiLevelCommentResponse, GameLevelComment>(comments, dataContext);
    }

    [ApiV3Endpoint("levels/id/{id}/comments", HttpMethods.Post)]
    [DocSummary("Posts the given comment under the specified level.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiResponse<ApiLevelCommentResponse> PostCommentOnLevel(RequestContext context,
        DataContext dataContext, int id, GameUser user, ApiCommentPostRequest body)
    {
        GameLevel? level = dataContext.Database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        // Trim content
        if (body.Content.Length > UgcLimits.CommentLimit) 
            body.Content = body.Content[..UgcLimits.CommentLimit];

        GameLevelComment comment = dataContext.Database.PostCommentToLevel(level, user, body.Content);
        return ApiLevelCommentResponse.FromOld(comment, dataContext);
    }

    [ApiV3Endpoint("levelComments/id/{id}"), Authentication(false)]
    [DocSummary("Gets the level comment specified by its ID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.CommentMissingErrorWhen)]
    public ApiResponse<ApiLevelCommentResponse> GetLevelComment(RequestContext context, DataContext dataContext, int id)
    {
        GameLevelComment? comment = dataContext.Database.GetLevelCommentById(id);
        if (comment == null) return ApiNotFoundError.CommentMissingError;

        return ApiLevelCommentResponse.FromOld(comment, dataContext);
    }

    [ApiV3Endpoint("levelComments/id/{id}", HttpMethods.Delete)]
    [DocSummary("Deletes the level comment specified by its ID. Fails if the user is not the comment poster or the level publisher.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.CommentMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.NoCommentDeletionPermissionErrorWhen)]
    public ApiOkResponse DeleteLevelComment(RequestContext context, DataContext dataContext, GameUser user, int id)
    {
        GameLevelComment? comment = dataContext.Database.GetLevelCommentById(id);
        if (comment == null) return ApiNotFoundError.CommentMissingError;

        if (user != comment.Author && user != comment.Level.Publisher) return ApiValidationError.NoCommentDeletionPermissionError;

        dataContext.Database.DeleteLevelComment(comment);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("levelComments/id/{id}/rate/{rawRating}", HttpMethods.Post)]
    [DocSummary("Rates the level comment specified by its ID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.CommentMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.RatingParseErrorWhen)]
    public ApiOkResponse RateLevelComment(RequestContext context, DataContext dataContext, GameUser user, int id, 
        [DocSummary("The user's new rating for the comment. -1 = dislike, 0 = neutral, 1 = like.")] string rawRating)
    {
        GameLevelComment? comment = dataContext.Database.GetLevelCommentById(id);
        if (comment == null) return ApiNotFoundError.CommentMissingError;

        // rawRating is string and not sbyte or integer because passing any out of range value will make Bunkum 
        // set rawRating to 0 instead, which we would here wrongly take as a neutral rating instead of an invalid value.
        if (!sbyte.TryParse(rawRating, out sbyte rating) || !Enum.IsDefined(typeof(RatingType), rating))
            return ApiValidationError.RatingParseError;

        dataContext.Database.RateLevelComment(user, comment, (RatingType)rating);
        return new ApiOkResponse();
    }
    #endregion
} 