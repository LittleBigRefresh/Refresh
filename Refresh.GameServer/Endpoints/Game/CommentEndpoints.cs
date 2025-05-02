using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Configuration;
using Refresh.Database;
using Refresh.Database.Models.Comments;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Lists;
using Refresh.Database.Models.Users;
using Refresh.GameServer.Types.Comments;
using Refresh.Database.Models.Levels;

namespace Refresh.GameServer.Endpoints.Game;

public class CommentEndpoints : EndpointGroup
{
    [GameEndpoint("postUserComment/{username}", ContentType.Xml, HttpMethods.Post)]
    [RequireEmailVerified]
    public Response PostProfileComment(RequestContext context, GameDatabaseContext database, string username, SerializedComment body, GameUser user, IDateTimeProvider timeProvider, GameServerConfig config)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;
        
        if (body.Content.Length > 4096)
        {
            return BadRequest;
        }

        GameUser? profile = database.GetUserByUsername(username);
        if (profile == null) return NotFound;
        
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

        return new SerializedCommentList(SerializedComment.FromOldList(database.GetProfileComments(profile, count, skip), dataContext));
    }

    [GameEndpoint("deleteUserComment/{username}", HttpMethods.Post)]
    public Response DeleteProfileComment(RequestContext context, GameDatabaseContext database, string username, GameUser user)
    {
        if (!int.TryParse(context.QueryString["commentId"], out int commentId)) return BadRequest;

        GameUser? profile = database.GetUserByUsername(username);
        if (profile == null) return NotFound;

        GameProfileComment? comment = database.GetProfileCommentById(commentId);
        if (comment == null) return BadRequest;

        //Validate someone doesnt try to delete someone elses comment
        if (comment.Author.UserId != user.UserId)
        {
            context.Logger.LogWarning(BunkumCategory.Game, $"User {user.Username} attempted to delete someone elses comment! This is likely a forged request");
            return Unauthorized;
        }

        database.DeleteProfileComment(comment, profile);

        return OK;
    }

    [GameEndpoint("postComment/{slotType}/{id}", ContentType.Xml, HttpMethods.Post)]
    [RequireEmailVerified]
    public Response PostLevelComment(RequestContext context, GameDatabaseContext database, string slotType, int id,
        SerializedComment body, GameUser user, GameServerConfig config)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;

        if (body.Content.Length > 4096)
        {
            return BadRequest;
        }
        
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return NotFound;

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

        return new SerializedCommentList(SerializedComment.FromOldList(database.GetLevelComments(level, count, skip), dataContext));
    }

    [GameEndpoint("deleteComment/{slotType}/{id}", HttpMethods.Post)]
    public Response DeleteLevelComment(RequestContext context, GameDatabaseContext database, string slotType, int id, GameUser user)
    {
        if (!int.TryParse(context.QueryString["commentId"], out int commentId)) return BadRequest;
        
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return NotFound;

        GameLevelComment? comment = database.GetLevelCommentById(commentId);
        if (comment == null) return BadRequest;
        
        //Validate someone doesnt try to delete someone else's comment
        if (comment.Author.UserId != user.UserId)
        {
            context.Logger.LogWarning(BunkumCategory.Game, $"User {user.Username} attempted to delete someone else's comment! This is likely a forged request");
            return Unauthorized;
        }
        
        database.DeleteLevelComment(comment, level);
        
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

        if (!database.RateProfileComment(user, comment, ratingType))
            return BadRequest;

        return OK;
    }
    
    [GameEndpoint("rateComment/user/{content}", HttpMethods.Post)] // `user` level comments
    [GameEndpoint("rateComment/developer/{content}", HttpMethods.Post)] // `developer` level comments
    public Response RateLevelComment(RequestContext context, GameDatabaseContext database, GameUser user, string content)
    {
        if (!int.TryParse(context.QueryString["commentId"], out int commentId)) return BadRequest;
        if (!Enum.TryParse(context.QueryString["rating"], out RatingType ratingType)) return BadRequest;

        GameLevelComment? comment = database.GetLevelCommentById(commentId);
        if (comment == null)
            return NotFound;

        if (!database.RateLevelComment(user, comment, ratingType))
            return BadRequest;

        return OK;
    }
}