using System.Text.RegularExpressions;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Documentation.Attributes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3;

public partial class AuthenticationApiEndpoints : EndpointGroup
{
    // How many rounds to do for password hashing (BCrypt)
    // 14 is ~1 second for logins and reset, which is fair because logins are a one-time thing
    // 200 OK on POST '/api/V3/resetPassword' (1058ms)
    // 200 OK on POST '/api/V3/auth' (1087ms)
    //
    // If increased, passwords will automatically be rehashed at login time to use the new WorkFactor
    // If decreased, passwords will stay at higher WorkFactor until reset
    private const int WorkFactor = 14;

    [GeneratedRegex("^[a-f0-9]{128}$")]
    private static partial Regex Sha512Regex();

    [ApiV3Endpoint("login", Method.Post), Authentication(false)]
    public ApiResponse<IApiAuthenticationResponse> Authenticate(RequestContext context, GameDatabaseContext database, ApiAuthenticationRequest body)
    {
        GameUser? user = database.GetUserByUsername(body.Username);
        if (user == null) return new ApiAuthenticationError("The username or password was incorrect.");

        // if this is a legacy user, have them create a password on login
        if (user.PasswordBcrypt == null)
        {
            Token resetToken = database.GenerateTokenForUser(user, TokenType.PasswordReset, TokenGame.Website, TokenPlatform.Website);

            return new ApiResetPasswordResponse
            {
                Reason = "The account you are trying to sign into is a legacy account. Please set a password.",
                ResetToken = resetToken.TokenData,
            };
        }

        if (BC.PasswordNeedsRehash(user.PasswordBcrypt, WorkFactor))
        {
            database.SetUserPassword(user, BC.HashPassword(body.PasswordSha512, WorkFactor));
        }

        if (!BC.Verify(body.PasswordSha512, user.PasswordBcrypt))
        {
            return new ApiAuthenticationError("The username or password was incorrect.");
        }

        Token token = database.GenerateTokenForUser(user, TokenType.Api, TokenGame.Website, TokenPlatform.Website);

        return new ApiAuthenticationResponse
        {
            TokenData = token.TokenData,
            UserId = user.UserId.ToString(),
            ExpiresAt = token.ExpiresAt,
        };
    }

    [ApiV3Endpoint("resetPassword", Method.Post), Authentication(false)]
    public ApiResponse<ApiOkResponse> ResetPassword(RequestContext context, GameDatabaseContext database, ApiResetPasswordRequest body)
    {
        GameUser? user = database.GetUserFromTokenData(body.ResetToken, TokenType.PasswordReset);
        if (user == null) return new ApiAuthenticationError("The reset token is invalid");

        if (body.PasswordSha512.Length != 128 || !Sha512Regex().IsMatch(body.PasswordSha512))
            return new ApiValidationError("Password is definitely not SHA512. Please hash the password - it'll work out better for both of us.");
        
        string? passwordBcrypt = BC.HashPassword(body.PasswordSha512, WorkFactor);
        if (passwordBcrypt == null) return new ApiInternalError("Could not hash the given password.");

        database.SetUserPassword(user, passwordBcrypt);

        return new ApiOkResponse();
    }

    [ApiV3Endpoint("logout", Method.Post)]
    [DocSummary("Tells the server to revoke the token used to make this request. Useful for logout behavior.")]
    public ApiOkResponse RevokeThisToken(RequestContext context, GameDatabaseContext database, Token token)
    {
        database.RevokeToken(token);
        return new ApiOkResponse();
    }
}