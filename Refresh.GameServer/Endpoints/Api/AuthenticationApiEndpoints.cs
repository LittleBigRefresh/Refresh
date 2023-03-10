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
        
        // if this is a legacy user, have them create a password on login
        if (user.PasswordBcrypt == null)
        {
            ResetToken resetToken = database.GenerateResetTokenForUser(user);

            ApiResetPasswordResponse resetResp = new()
            {
                Reason = "The account you are trying to sign into is a legacy account. Please set a password.",
                ResetToken = resetToken.TokenData,
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

    [ApiEndpoint("resetPassword", Method.Post)]
    [Authentication(false)]
    public Response ResetPassword(RequestContext context, RealmDatabaseContext database, ApiResetPasswordRequest body)
    {
        GameUser? user = database.GetUserFromResetTokenData(body.ResetToken);
        if (user == null) return new Response(HttpStatusCode.Unauthorized);

        if (body.PasswordSha512.Length != 128)
            return new Response("Password is definitely not SHA512. Please hash the password - it'll work out better for both of us.",
                ContentType.Plaintext, HttpStatusCode.BadRequest);
        
        string? passwordBcrypt = BCrypt.Net.BCrypt.HashPassword(body.PasswordSha512);
        if (passwordBcrypt == null) return new Response(HttpStatusCode.InternalServerError);

        database.SetUserPassword(user, passwordBcrypt);

        return new Response(HttpStatusCode.OK);
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

public class ApiResetPasswordRequest
{
    public string PasswordSha512 { get; set; }
    public string ResetToken { get; set; }
}

public class ApiResetPasswordResponse
{
    public string Reason { get; set; }
    public string ResetToken { get; set; }
}