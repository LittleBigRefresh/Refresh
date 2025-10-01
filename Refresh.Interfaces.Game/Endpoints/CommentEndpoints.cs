using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Common.Constants;
using Refresh.Common.Time;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.Comments;
using Refresh.Interfaces.Game.Types.Lists;

namespace Refresh.Interfaces.Game.Endpoints;

public class CommentEndpoints : EndpointGroup
{
    [GameEndpoint("postUserComment/{username}", ContentType.Xml, HttpMethods.Post)]
    [RequireEmailVerified]
    public Response PostProfileComment(RequestContext context, GameDatabaseContext database, string username, SerializedComment body, GameUser user, IDateTimeProvider timeProvider, GameServerConfig config)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;
        
        GameUser? profile = database.GetUserByUsername(username);
        if (profile == null) return NotFound;

        if (body.Content.Length > UgcLimits.CommentLimit)
        {
            database.AddErrorNotification("Failed to post comment", $"Your comment under {profile.Username}'s profile couldn't be posted because it was too long.", user);
            return BadRequest;
        }
        
        // TODO: include a check for if the user wants to receive these types of notifications 
        if (!profile.Equals(user)) 
        {
            database.AddNotification("New comment", $"{user.Username} left a comment on your profile!", profile);
        }

        database.PostCommentToProfile(profile, user, body.Content);
        return OK;
    }

    [GameEndpoint("userComments/{username}", ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedCommentList? GetProfileComments(RequestContext context, GameDatabaseContext database, GameUser user, DataContext dataContext, string username)
    {
        GameUser? profile = database.GetUserByUsername(username);
        if (profile == null) return null;
        
        (int skip, int count) = context.GetPageData();

        DatabaseList<GameProfileComment> comments = database.GetProfileComments(profile, count, skip);
        return new SerializedCommentList(SerializedComment.FromOldList(comments.Items.ToArray(), dataContext));
    }

    [GameEndpoint("deleteUserComment/{username}", HttpMethods.Post)]
    public Response DeleteProfileComment(RequestContext context, GameDatabaseContext database, string username, GameUser user)
    {
        if (!int.TryParse(context.QueryString["commentId"], out int commentId)) return BadRequest;

        GameUser? profile = database.GetUserByUsername(username);
        if (profile == null) return NotFound;

        GameProfileComment? comment = database.GetProfileCommentById(commentId);
        if (comment == null) return BadRequest;

        // Validate someone doesnt try to delete someone elses comment.
        // Also allow profile owners to delete any comment off their profile to not make the game make it look like us not implementing this is a bug.
        if (user.UserId != comment.AuthorUserId && user.UserId != profile.UserId)
        {
            context.Logger.LogWarning(BunkumCategory.Game, $"User {user.Username} attempted to delete someone elses comment! This is likely a forged request");
            return Unauthorized;
        }

        database.DeleteProfileComment(comment);

        return OK;
    }

    [GameEndpoint("postComment/{slotType}/{id}", ContentType.Xml, HttpMethods.Post)]
    [RequireEmailVerified]
    public Response PostLevelComment(RequestContext context, GameDatabaseContext database, string slotType, int id,
        SerializedComment body, GameUser user, GameServerConfig config)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;

        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return NotFound;

        if (body.Content.Length > UgcLimits.CommentLimit)
        {
            database.AddErrorNotification("Failed to post comment", $"Your comment under the level '{level.Title}' couldn't be posted because it was too long.", user);
            return BadRequest;
        }

        if (level.Publisher != null && !level.Publisher.Equals(user)) 
        {
            database.AddNotification("New comment", $"{user.Username} left a comment on your level: '{level.Title}!'", level.Publisher);
        }

        database.PostCommentToLevel(level, user, body.Content);
        return OK;
    }

    [GameEndpoint("comments/{slotType}/{id}", ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedCommentList? GetLevelComments(RequestContext context, GameDatabaseContext database, GameUser user, DataContext dataContext,
        string slotType, int id)
    {
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return null;

        (int skip, int count) = context.GetPageData();

        DatabaseList<GameLevelComment> comments = database.GetLevelComments(level, count, skip);
        return new SerializedCommentList(SerializedComment.FromOldList(comments.Items.ToArray(), dataContext));
    }

    [GameEndpoint("deleteComment/{slotType}/{id}", HttpMethods.Post)]
    public Response DeleteLevelComment(RequestContext context, GameDatabaseContext database, string slotType, int id, GameUser user)
    {
        if (!int.TryParse(context.QueryString["commentId"], out int commentId)) return BadRequest;
        
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return NotFound;

        GameLevelComment? comment = database.GetLevelCommentById(commentId);
        if (comment == null) return BadRequest;
        
        // Validate someone doesnt try to delete someone else's comment.
        // Also allow level publishers to delete any comment off their level to not make the game make it look like us not implementing this is a bug.
        if (comment.AuthorUserId != user.UserId && user.UserId != level.PublisherUserId)
        {
            context.Logger.LogWarning(BunkumCategory.Game, $"User {user.Username} attempted to delete someone else's comment! This is likely a forged request");
            return Unauthorized;
        }
        
        database.DeleteLevelComment(comment);
        
        return OK;
    }
    
    [GameEndpoint("rateUserComment/{content}", HttpMethods.Post)] // profile comments
    public Response RateProfileComment(RequestContext context, GameDatabaseContext database, GameUser user, string content)
    {
        if (!int.TryParse(context.QueryString["commentId"], out int commentId)) return BadRequest;
        if (!Enum.TryParse(context.QueryString["rating"], out RatingType ratingType)) return BadRequest;

        GameProfileComment? comment = database.GetProfileCommentById(commentId);
        if (comment == null)
            return NotFound;

        database.RateProfileComment(user, comment, ratingType);
        return OK;
    }
    
    [GameEndpoint("rateComment/{slotType}/{content}", HttpMethods.Post)]
    public Response RateLevelComment(RequestContext context, GameDatabaseContext database, GameUser user, string slotType, string content)
    {
        if (!int.TryParse(context.QueryString["commentId"], out int commentId)) return BadRequest;
        if (!Enum.TryParse(context.QueryString["rating"], out RatingType ratingType)) return BadRequest;

        if (slotType is not "user" or "developer") return BadRequest;

        GameLevelComment? comment = database.GetLevelCommentById(commentId);
        if (comment == null)
            return NotFound;

        database.RateLevelComment(user, comment, ratingType);
        return OK;
    }
}