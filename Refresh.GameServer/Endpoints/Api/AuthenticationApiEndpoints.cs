using System.Net;
using System.Security.Cryptography;
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
        
        // if this is a legacy user, have them create a password on login
        if (user.PasswordBcrypt == null)
        {
            byte[] tokenData = new byte[128];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) 
                rng.GetBytes(tokenData);

            ApiResetPasswordResponse resetResp = new()
            {
                Reason = "The account you are trying to sign into is a legacy account. Please set a password.",
                ResetToken = Convert.ToBase64String(tokenData),
            };

            return new Response(resetResp, ContentType.Json, HttpStatusCode.Unauthorized);
        }
        
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
    public string PasswordBcrypt { get; set; }
}

[Serializable]
public class ApiAuthenticationResponse
{
    public string TokenData { get; set; }
    public string UserId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}

public class ApiResetPasswordResponse
{
    public string Reason { get; set; }
    public string ResetToken { get; set; }
}