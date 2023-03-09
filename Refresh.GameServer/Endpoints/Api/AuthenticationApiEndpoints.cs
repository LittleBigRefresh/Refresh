using System.Net;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Api;

public class AuthenticationApiEndpoints : EndpointGroup
{
    [ApiEndpoint("auth", Method.Post)]
    [Authentication(false)]
    public Response Authenticate(RequestContext context, RealmDatabaseContext database, ApiAuthenticationRequest body)
    {
        GameUser? user = database.GetUserByUsername(body.Username);
        if (user == null) return new Response(HttpStatusCode.NotFound);
        
        Token token = database.GenerateTokenForUser(user, TokenType.Api);

        ApiAuthenticationResponse resp = new()
        {
            TokenData = token.TokenData,
            UserId = user.UserId.ToString(),
            ExpiresAt = token.ExpiresAt,
        };

        return new Response(resp, ContentType.Json);
    }
}

#nullable disable

[Serializable]
public class ApiAuthenticationRequest
{
    public string Username { get; set; }
    public string PasswordSha512 { get; set; }
}

[Serializable]
public class ApiAuthenticationResponse
{
    public string TokenData { get; set; }
    public string UserId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}