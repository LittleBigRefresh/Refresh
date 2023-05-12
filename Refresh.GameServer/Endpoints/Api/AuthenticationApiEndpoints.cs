using System.Net;
using System.Text.RegularExpressions;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Api;

public partial class AuthenticationApiEndpoints : EndpointGroup
{
    // How many rounds to do for password hashing (BCrypt)
    // 14 is ~1 second for logins and reset, which is fair because logins are a one-time thing
    // 200 OK on POST '/api/v2/resetPassword' (1058ms)
    // 200 OK on POST '/api/v2/auth' (1087ms)
    //
    // If increased, passwords will automatically be rehashed at login time to use the new WorkFactor
    // If decreased, passwords will stay at higher WorkFactor until reset
    private const int WorkFactor = 14;

    [GeneratedRegex("^[a-f0-9]{128}$")]
    private static partial Regex Sha512Regex();

    [ApiEndpoint("auth", Method.Post)]
    [Authentication(false)]
    public Response Authenticate(RequestContext context, GameDatabaseContext database, ApiAuthenticationRequest body)
    {
        GameUser? user = database.GetUserByUsername(body.Username);
        if (user == null)
        {
            return new Response(new ApiErrorResponse("The username or password was incorrect."), ContentType.Json, Forbidden);
        }

        // if this is a legacy user, have them create a password on login
        if (user.PasswordBcrypt == null)
        {
            Token resetToken = database.GenerateTokenForUser(user, TokenType.PasswordReset, TokenGame.Website, TokenPlatform.Website);

            ApiResetPasswordResponse resetResp = new()
            {
                Reason = "The account you are trying to sign into is a legacy account. Please set a password.",
                ResetToken = resetToken.TokenData,
            };

            return new Response(resetResp, ContentType.Json, Unauthorized);
        }

        if (BC.PasswordNeedsRehash(user.PasswordBcrypt, WorkFactor))
        {
            database.SetUserPassword(user, BC.HashPassword(body.PasswordSha512, WorkFactor));
        }

        if (!BC.Verify(body.PasswordSha512, user.PasswordBcrypt))
        {
            return new Response(new ApiErrorResponse("The username or password was incorrect."), ContentType.Json, Forbidden);
        }

        Token token = database.GenerateTokenForUser(user, TokenType.Api, TokenGame.Website, TokenPlatform.Website);

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
    public Response ResetPassword(RequestContext context, GameDatabaseContext database, ApiResetPasswordRequest body)
    {
        GameUser? user = database.GetUserFromTokenData(body.ResetToken, TokenType.PasswordReset);
        if (user == null) return Unauthorized;

        if (body.PasswordSha512.Length != 128 || !Sha512Regex().IsMatch(body.PasswordSha512))
            return new Response("Password is definitely not SHA512. Please hash the password - it'll work out better for both of us.",
                ContentType.Plaintext, BadRequest);
        
        string? passwordBcrypt = BC.HashPassword(body.PasswordSha512, WorkFactor);
        if (passwordBcrypt == null) return InternalServerError;

        database.SetUserPassword(user, passwordBcrypt);

        return OK;
    }

    [ApiEndpoint("goodbye", Method.Post)]
    [Authentication(false)]
    public Response RevokeThisToken(RequestContext context, GameDatabaseContext database)
    {
        bool success = database.RevokeTokenByTokenData(context.RequestHeaders["Authorization"], TokenType.Api);

        if (success) return OK;
        return Unauthorized;
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

[Serializable]
public class ApiResetPasswordRequest
{
    public string PasswordSha512 { get; set; }
    public string ResetToken { get; set; }
}

[Serializable]
public class ApiResetPasswordResponse
{
    public string Reason { get; set; }
    public string ResetToken { get; set; }
}

[Serializable]
public class ApiErrorResponse
{
    public ApiErrorResponse() {}

    public ApiErrorResponse(string reason)
    {
        this.Reason = reason;
    }
    
    public string Reason { get; set; }
}