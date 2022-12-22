using System.Net;
using JetBrains.Annotations;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Comments;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;
using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints;

public class CommentEndpoints : EndpointGroup
{
    [GameEndpoint("postUserComment/{username}", ContentType.Xml)]
    public Response PostProfileComment(RequestContext context, RealmDatabaseContext database, string username, GameComment body, GameUser user)
    {
        GameUser? profile = database.GetUser(username);
        if (profile == null) return new Response(HttpStatusCode.NotFound);
        
        database.PostCommentToProfile(profile, user, body.Content);
        return new Response(HttpStatusCode.OK);
    }

    [GameEndpoint("userComments/{username}", ContentType.Xml)]
    [NullStatusCode(HttpStatusCode.NotFound)]
    public GameCommentList? GetProfileComments(RequestContext context, RealmDatabaseContext database, string username)
    {
        GameUser? profile = database.GetUser(username);
        if (profile == null) return null;
        
        (int skip, int count) = this.GetPageData(context);

        List<GameComment> comments = database.GetProfileComments(profile, count, skip).ToList();
        foreach (GameComment comment in comments) comment.PrepareForSerialization();

        return new GameCommentList(comments);
    }
    
    // TODO: move this to an extension method on RequestContext? this is duplicated from LevelEndpoints.cs
    [Pure]
    private (int, int) GetPageData(RequestContext context)
    {
        int.TryParse(context.Request.QueryString["pageStart"], out int skip);
        if (skip != default) skip--;
        
        int.TryParse(context.Request.QueryString["pageSize"], out int count);
        if (count == default) count = 20;

        return (skip, count);
    }
}