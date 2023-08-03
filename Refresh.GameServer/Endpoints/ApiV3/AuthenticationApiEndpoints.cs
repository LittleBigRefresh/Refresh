using System.Net;
using System.Text.RegularExpressions;
using AttribDoc.Attributes;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Extensions;
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

    [ApiV3Endpoint("resetPassword", Method.Put), Authentication(false)]
    public ApiResponse<ApiOkResponse> ResetPassword(RequestContext context, GameDatabaseContext database, ApiResetPasswordRequest body)
    {
        GameUser? user = database.GetUserFromTokenData(body.ResetToken, TokenType.PasswordReset);
        if (user == null) return new ApiAuthenticationError("The reset token is invalid");

        if (body.PasswordSha512.Length != 128 || !Sha512Regex().IsMatch(body.PasswordSha512))
            return new ApiValidationError("Password is definitely not SHA512. Please hash the password.");
        
        string? passwordBcrypt = BC.HashPassword(body.PasswordSha512, WorkFactor);
        if (passwordBcrypt == null) return new ApiInternalError("Could not BCrypt the given password.");

        database.SetUserPassword(user, passwordBcrypt);
        database.RevokeTokenByTokenData(body.ResetToken, TokenType.PasswordReset);

        return new ApiOkResponse();
    }

    [ApiV3Endpoint("logout", Method.Put)]
    [DocSummary("Tells the server to revoke the token used to make this request. Useful for logout behavior.")]
    public ApiOkResponse RevokeThisToken(RequestContext context, GameDatabaseContext database, Token token)
    {
        database.RevokeToken(token);
        return new ApiOkResponse();
    }
    
    // IP Verification
    [ApiV3Endpoint("verificationRequests")]
    [DocSummary("Retrieves a list of IP addresses that have attempted to connect.")]
    public ApiListResponse<ApiGameIpVerificationRequestResponse> GetVerificationRequests(RequestContext context, GameDatabaseContext database, GameUser user)
    {
        (int skip, int count) = context.GetPageData(true);

        return DatabaseList<ApiGameIpVerificationRequestResponse>.FromOldList<ApiGameIpVerificationRequestResponse, GameIpVerificationRequest>
                (database.GetIpVerificationRequestsForUser(user, count, skip));
    }

    [ApiV3Endpoint("verificationRequests/approve", Method.Put)]
    [DocSummary("Approves a given IP, and clears all remaining verification requests. Send the IP in the body.")]
    [DocError(typeof(ApiValidationError), ApiValidationError.IpAddressParseErrorWhen)]
    [DocRequestBody("127.0.0.1")]
    public ApiOkResponse ApproveVerificationRequest(RequestContext context, GameDatabaseContext database, GameUser user, string body)
    {
        bool parsed = IPAddress.TryParse(body, out _);
        if (!parsed) return ApiValidationError.IpAddressParseError;

        database.SetApprovedIp(user, body.Trim());
        
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("verificationRequests/deny", Method.Put)]
    [DocSummary("Denies all verification requests matching a given IP. Send the IP in the body.")]
    [DocError(typeof(ApiValidationError), ApiValidationError.IpAddressParseErrorWhen)]
    [DocRequestBody("127.0.0.1")]
    public ApiOkResponse DenyVerificationRequest(RequestContext context, GameDatabaseContext database, GameUser user, string body)
    {
        bool parsed = IPAddress.TryParse(body, out _);
        if (!parsed) return ApiValidationError.IpAddressParseError;

        database.DenyIpVerificationRequest(user, body.Trim());
        
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("register", Method.Post), Authentication(false)]
    [DocSummary("Registers a new user.")]
    [DocRequestBody(typeof(ApiRegisterRequest))]
    public ApiResponse<IApiAuthenticationResponse> Register(RequestContext context, GameDatabaseContext database, ApiRegisterRequest body, GameServerConfig config)
    {
        if (!config.RegistrationEnabled)
            return new ApiAuthenticationError("Registration is not enabled on this server.");
            
        if (body.PasswordSha512.Length != 128 || !Sha512Regex().IsMatch(body.PasswordSha512))
            return new ApiValidationError("Password is definitely not SHA512. Please hash the password.");
        
        string? passwordBcrypt = BC.HashPassword(body.PasswordSha512, WorkFactor);
        if (passwordBcrypt == null) return new ApiInternalError("Could not BCrypt the given password.");

        GameUser user = database.CreateUser(body.Username);
        database.SetUserPassword(user, passwordBcrypt);
        
        Token token = database.GenerateTokenForUser(user, TokenType.Api, TokenGame.Website, TokenPlatform.Website);

        return new ApiAuthenticationResponse
        {
            TokenData = token.TokenData,
            UserId = user.UserId.ToString(),
            ExpiresAt = token.ExpiresAt,
        };
    }
}