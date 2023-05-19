using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Comments;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class CommentEndpoints : EndpointGroup
{
    [GameEndpoint("postUserComment/{username}", ContentType.Xml, Method.Post)]
    public Response PostProfileComment(RequestContext context, GameDatabaseContext database, string username, GameComment body, GameUser user)
    {
        GameUser? profile = database.GetUserByUsername(username);
        if (profile == null) return NotFound;
        
        database.PostCommentToProfile(profile, user, body.Content);
        return OK;
    }

    [GameEndpoint("userComments/{username}", ContentType.Xml)]
    [NullStatusCode(NotFound)]
    public SerializedCommentList? GetProfileComments(RequestContext context, GameDatabaseContext database, string username)
    {
        GameUser? profile = database.GetUserByUsername(username);
        if (profile == null) return null;
        
        (int skip, int count) = context.GetPageData();

        List<GameComment> comments = database.GetProfileComments(profile, count, skip).ToList();
        foreach (GameComment comment in comments) comment.PrepareForSerialization();

        return new SerializedCommentList(comments);
    }
}