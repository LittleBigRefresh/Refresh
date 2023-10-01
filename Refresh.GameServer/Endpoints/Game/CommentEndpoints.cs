using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Comments;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class CommentEndpoints : EndpointGroup
{
    [GameEndpoint("postUserComment/{username}", ContentType.Xml, HttpMethods.Post)]
    public Response PostProfileComment(RequestContext context, GameDatabaseContext database, string username, GameComment body, GameUser user, IDateTimeProvider timeProvider)
    {
        if (body.Content.Length > 4096)
        {
            return BadRequest;
        }

        GameUser? profile = database.GetUserByUsername(username);
        if (profile == null) return NotFound;
        
        database.PostCommentToProfile(profile, user, body.Content);
        return OK;
    }

    [GameEndpoint("userComments/{username}", ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedCommentList? GetProfileComments(RequestContext context, GameDatabaseContext database, string username)
    {
        GameUser? profile = database.GetUserByUsername(username);
        if (profile == null) return null;
        
        (int skip, int count) = context.GetPageData();

        List<GameComment> comments = database.GetProfileComments(profile, count, skip).ToList();
        foreach (GameComment comment in comments) comment.PrepareForSerialization();

        return new SerializedCommentList(comments);
    }

    [GameEndpoint("deleteUserComment/{username}", HttpMethods.Post)]
    public Response DeleteProfileComment(RequestContext context, GameDatabaseContext database, string username, GameUser user)
    {
        if (!int.TryParse(context.QueryString["commentId"], out int commentId)) return BadRequest;

        GameUser? profile = database.GetUserByUsername(username);
        if (profile == null) return NotFound;

        GameComment? comment = profile.ProfileComments.FirstOrDefault(comment => comment.SequentialId == commentId);
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

    [GameEndpoint("postComment/user/{levelId}", ContentType.Xml, HttpMethods.Post)]
    public Response PostLevelComment(RequestContext context, GameDatabaseContext database, int levelId, GameComment body, GameUser user)
    {
        if (body.Content.Length > 4096)
        {
            return BadRequest;
        }
        
        GameLevel? level = database.GetLevelById(levelId);
        if (level == null) return NotFound;

        database.PostCommentToLevel(level, user, body.Content);
        return OK;
    }

    [GameEndpoint("comments/user/{levelId}", ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedCommentList? GetLevelComments(RequestContext context, GameDatabaseContext database, int levelId)
    {
        GameLevel? level = database.GetLevelById(levelId);
        if (level == null) return null;

        (int skip, int count) = context.GetPageData();

        List<GameComment> comments = database.GetLevelComments(level, count, skip).ToList();
        foreach(GameComment comment in comments) comment.PrepareForSerialization();

        return new SerializedCommentList(comments);
    }

    [GameEndpoint("deleteComment/user/{levelId}", HttpMethods.Post)]
    public Response DeleteLevelComment(RequestContext context, GameDatabaseContext database, int levelId, GameUser user)
    {
        if (!int.TryParse(context.QueryString["commentId"], out int commentId)) return BadRequest;
        
        GameLevel? level = database.GetLevelById(levelId);
        if (level == null) return NotFound;
        
        GameComment? comment = level.LevelComments.FirstOrDefault(comment => comment.SequentialId == commentId);
        if (comment == null) return BadRequest;
        
        //Validate someone doesnt try to delete someone elses comment
        if (comment.Author.UserId != user.UserId)
        {
            context.Logger.LogWarning(BunkumCategory.Game, $"User {user.Username} attempted to delete someone elses comment! This is likely a forged request");
            return Unauthorized;
        }
        
        database.DeleteLevelComment(comment, level);
        
        return OK;
    }
}