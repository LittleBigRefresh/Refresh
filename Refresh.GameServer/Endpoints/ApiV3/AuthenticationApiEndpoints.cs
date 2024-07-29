using System.Net;
using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.RateLimit;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request.Authentication;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Users;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Verification;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class AuthenticationApiEndpoints : EndpointGroup
{
    /// <summary>
    ///     How many rounds to do for password hashing (BCrypt)
    ///     On my machine, a work factor of 14 takes roughly 1 second for password checks.
    /// </summary>
    /// <remarks>
    ///     If increased, passwords will automatically be rehashed at login time to use the new WorkFactor 
    ///     If decreased, passwords will stay at higher WorkFactor until reset
    /// </remarks>
    public const int WorkFactor = 14;
    
    /// <summary>
    /// A randomly generated password.
    /// Used to prevent against timing attacks.
    /// </summary>
    private static readonly string FakePassword = BC.HashPassword(Random.Shared.Next().ToString(), WorkFactor);

    [ApiV3Endpoint("login", HttpMethods.Post), Authentication(false), AllowDuringMaintenance]
    [DocRequestBody(typeof(ApiAuthenticationRequest))]
    [RateLimitSettings(300, 10, 300, "auth")]
    public ApiResponse<IApiAuthenticationResponse> Authenticate(RequestContext context, GameDatabaseContext database, ApiAuthenticationRequest body, GameServerConfig config)
    {
        GameUser? user = database.GetUserByEmailAddress(body.EmailAddress);
        if (user == null)
        {
            // Do the work of checking the password if there was no user found.
            // If we immediately return when we can't find a user, then it will be a short-lived request.
            // If we find a user and we check the password, then the request will take much longer.
            //
            // You can use this discrepancy to determine if a given email is valid.
            // Thus, we should always do the work of checking the password.
            _ = BC.Verify(body.PasswordSha512, FakePassword);
            
            return new ApiAuthenticationError("The email or password was incorrect.");
        }
        
        if (config.MaintenanceMode && user.Role != GameUserRole.Admin)
            return new ApiAuthenticationError(
                "The server is currently in maintenance mode, so it is only accessible for administrators. " +
                "Check back later.");
        
        string ipAddress = context.RemoteIp();
        
        // if this is a legacy user, have them create a password on login
        if (user.PasswordBcrypt == null)
        {
            Token resetToken = database.GenerateTokenForUser(user, TokenType.PasswordReset, TokenGame.Website, TokenPlatform.Website, ipAddress);

            return new ApiResetPasswordResponse
            {
                Reason = "The account you are trying to sign into is a legacy account. Please set a password.",
                ResetToken = resetToken.TokenData,
            };
        }

        if (!BC.Verify(body.PasswordSha512, user.PasswordBcrypt))
            return new ApiAuthenticationError("The email or password was incorrect.");
        
        if (BC.PasswordNeedsRehash(user.PasswordBcrypt, WorkFactor))
            database.SetUserPassword(user, BC.HashPassword(body.PasswordSha512, WorkFactor));
        
        if (user.Role == GameUserRole.Banned)
            return new ApiAuthenticationError($"You are banned until {user.BanExpiryDate.ToString()}. " +
                                              $"For more information or to request account deletion, please contact the server administrator.\n" +
                                              $"Reason: {user.BanReason}");

        Token token = database.GenerateTokenForUser(user, TokenType.Api, TokenGame.Website, TokenPlatform.Website, ipAddress);
        Token refreshToken = database.GenerateTokenForUser(user, TokenType.ApiRefresh, TokenGame.Website, TokenPlatform.Website, ipAddress, GameDatabaseContext.RefreshTokenExpirySeconds);
        
        context.Logger.LogInfo(BunkumCategory.Authentication, $"{user} successfully logged in through the API");

        return new ApiAuthenticationResponse
        {
            RefreshTokenData = refreshToken.TokenData,
            TokenData = token.TokenData,
            UserId = user.UserId.ToString(),
            ExpiresAt = token.ExpiresAt,
        };
    }

    [ApiV3Endpoint("refreshToken", HttpMethods.Post), Authentication(false), AllowDuringMaintenance]
    [DocRequestBody(typeof(ApiRefreshRequest))]
    [RateLimitSettings(300, 10, 300, "auth")]
    public ApiResponse<IApiAuthenticationResponse> RefreshToken(RequestContext context, GameDatabaseContext database, ApiRefreshRequest body)
    {
        Token? refreshToken = database.GetTokenFromTokenData(body.TokenData, TokenType.ApiRefresh);
        if (refreshToken == null) return new ApiAuthenticationError("Your session has expired, please sign in again.");

        GameUser user = refreshToken.User;

        Token token = database.GenerateTokenForUser(user, TokenType.Api, TokenGame.Website, TokenPlatform.Website, context.RemoteIp());
        
        context.Logger.LogInfo(BunkumCategory.Authentication, $"{user} successfully refreshed their token through the API");

        return new ApiAuthenticationResponse
        {
            RefreshTokenData = null,
            TokenData = token.TokenData,
            UserId = user.UserId.ToString(),
            ExpiresAt = token.ExpiresAt,
        };
    }

    [ApiV3Endpoint("resetPassword", HttpMethods.Put), Authentication(false)]
    [RateLimitSettings(300, 10, 300, "auth")]
    public ApiOkResponse ResetPassword(RequestContext context, GameDatabaseContext database, ApiResetPasswordRequest body, GameUser? user)
    {
        user ??= database.GetUserFromTokenData(body.ResetToken, TokenType.PasswordReset);
        if (user == null) return new ApiAuthenticationError("The reset token is invalid");

        if (body.PasswordSha512.Length != 128 || !CommonPatterns.Sha512Regex().IsMatch(body.PasswordSha512))
            return new ApiValidationError("Password is definitely not SHA512. Please hash the password.");
        
        string? passwordBcrypt = BC.HashPassword(body.PasswordSha512, WorkFactor);
        if (passwordBcrypt == null) return new ApiInternalError("Could not BCrypt the given password.");

        database.SetUserPassword(user, passwordBcrypt);
        database.RevokeTokenByTokenData(body.ResetToken, TokenType.PasswordReset);
        
        context.Logger.LogInfo(BunkumCategory.Authentication, $"{user} successfully reset their password");

        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("sendPasswordResetEmail", HttpMethods.Put), Authentication(false)]
    [RateLimitSettings(86400 / 2, 5, 86400, "resetPassword")]
    public ApiOkResponse SendPasswordResetEmail(RequestContext context,
        GameDatabaseContext database,
        ApiSendPasswordResetEmailRequest body,
        SmtpService smtpService)
    {
        GameUser? user = database.GetUserByEmailAddress(body.EmailAddress);
        if (user == null)
        {
            context.Logger.LogWarning(RefreshContext.PasswordReset, "Couldn't find a user by the email '{0}', not sending email", body.EmailAddress);
            // return a fake success on purpose
            return new ApiOkResponse();
        }
        
        context.Logger.LogInfo(RefreshContext.PasswordReset, "Sending a password reset request email to {0}.", user.Username);
        
        Token token = database.GenerateTokenForUser(user, TokenType.PasswordReset, TokenGame.Website, TokenPlatform.Website, context.RemoteIp());
        context.Logger.LogTrace(RefreshContext.PasswordReset, "Reset token: {0}", token.TokenData);
        smtpService.SendPasswordResetRequest(user, token.TokenData);

        context.Logger.LogInfo(RefreshContext.PasswordReset, "Email sent, token will expire at {0}", token.ExpiresAt);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("logout", HttpMethods.Put), MinimumRole(GameUserRole.Restricted)]
    [DocSummary("Tells the server to revoke the token used to make this request. Useful for logout behavior.")]
    public ApiOkResponse RevokeThisToken(RequestContext context, GameDatabaseContext database, Token token)
    {
        context.Logger.LogInfo(BunkumCategory.Authentication, $"{token.User} logged out");
        database.RevokeToken(token);
        return new ApiOkResponse();
    }
    
    // IP Verification
    [ApiV3Endpoint("verificationRequests"), MinimumRole(GameUserRole.Restricted)]
    [DocSummary("Retrieves a list of IP addresses that have attempted to connect.")]
    public ApiListResponse<ApiGameIpVerificationRequestResponse> GetVerificationRequests(RequestContext context,
        GameDatabaseContext database, GameUser user, DataContext dataContext)
    {
        (int skip, int count) = context.GetPageData();

        return DatabaseList<ApiGameIpVerificationRequestResponse>.FromOldList<ApiGameIpVerificationRequestResponse, GameIpVerificationRequest>
                (database.GetIpVerificationRequestsForUser(user, count, skip), dataContext);
    }

    [ApiV3Endpoint("verificationRequests/approve", HttpMethods.Put)]
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
    
    [ApiV3Endpoint("verificationRequests/deny", HttpMethods.Put)]
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

    [ApiV3Endpoint("register", HttpMethods.Post), Authentication(false)]
    [DocSummary("Registers a new user.")]
    [DocRequestBody(typeof(ApiRegisterRequest))]
    #if !DEBUG
    [RateLimitSettings(3600, 5, 3600 / 2, "register")]
    #endif
    public ApiResponse<IApiAuthenticationResponse> Register(RequestContext context,
        GameDatabaseContext database,
        ApiRegisterRequest body,
        GameServerConfig config,
        IntegrationConfig integrationConfig,
        SmtpService smtpService)
    {
        if (!config.RegistrationEnabled)
            return new ApiAuthenticationError("Registration is not enabled on this server. Check back later.");
            
        if (body.PasswordSha512.Length != 128 || !CommonPatterns.Sha512Regex().IsMatch(body.PasswordSha512))
            return new ApiValidationError("Password is definitely not SHA512. Please hash the password.");

        if (!CommonPatterns.EmailAddressRegex().IsMatch(body.EmailAddress))
            return new ApiValidationError("The email address given is invalid.");
        
        if (!smtpService.CheckEmailDomainValidity(body.EmailAddress))
            return ApiValidationError.EmailDoesNotActuallyExistError;
        
        if (database.IsUserDisallowed(body.Username))
            return new ApiAuthenticationError("This username is disallowed from being registered.");
        
        if (!database.IsUsernameValid(body.Username))
            return new ApiValidationError(
                "The username must be valid. " +
                "The requirements are 3 to 16 alphanumeric characters, plus hyphens and underscores. " +
                "Are you sure you used your PSN/RPCN username?");
        
        if (database.IsUsernameTaken(body.Username) || database.IsEmailTaken(body.EmailAddress))
        {
            return new ApiAuthenticationError(
                "The account could not be registered because username or email was already taken. " +
                (config.RequireGameLoginToRegister ? "If you have already registered, try signing in via the game to activate your account." : ""));
        }

        string? passwordBcrypt = BC.HashPassword(body.PasswordSha512, WorkFactor);
        if (passwordBcrypt == null) return new ApiInternalError("Could not BCrypt the given password.");

        if (config.RequireGameLoginToRegister)
        {
            database.AddRegistrationToQueue(body.Username, body.EmailAddress, passwordBcrypt);
            return new ApiAuthenticationError(
                "Your account has been put into the registration queue, but it is not yet activated. " +
                "To complete registration, patch your games to our servers and start playing within the next hour and your new account will be activated. " +
                "You will be unable to sign in until you are patched and playing. For more instructions on patching, please visit https://docs.littlebigrefresh.com", true);
        }

        GameUser user = database.CreateUser(body.Username, body.EmailAddress, true);
        database.SetUserPassword(user, passwordBcrypt);

        if (integrationConfig.SmtpEnabled)
        {
            EmailVerificationCode code = database.CreateEmailVerificationCode(user);
            smtpService.SendEmailVerificationRequest(user, code.Code);
        }
        else
        {
            // if smtp isn't enabled just mark the user's email as verified
            database.VerifyUserEmail(user);
        }
        
        Token token = database.GenerateTokenForUser(user, TokenType.Api, TokenGame.Website, TokenPlatform.Website, context.RemoteIp());

        return new ApiAuthenticationResponse
        {
            TokenData = token.TokenData,
            UserId = user.UserId.ToString(),
            ExpiresAt = token.ExpiresAt,
        };
    }
    [ApiV3Endpoint("verify", HttpMethods.Post)]
    [DocSummary("Verifies an email address using the given code")]
    public ApiOkResponse VerifyEmail(RequestContext context, GameUser user, GameDatabaseContext database)
    {
        string? code = context.QueryString.Get("code");
        if (code == null) return new ApiValidationError("The code parameter was not found or invalid");

        if (!database.VerificationCodeMatches(user, code.Trim())) return ApiNotFoundError.Instance;
        database.VerifyUserEmail(user);

        return new ApiOkResponse();
    }

    [ApiV3Endpoint("verify/resend", HttpMethods.Post)]
    [DocSummary("Instructs the server to resend the verification email with a new code")]
    public ApiOkResponse ResendVerificationCode(RequestContext context, GameUser user, GameDatabaseContext database, SmtpService smtpService)
    {
        EmailVerificationCode code = database.CreateEmailVerificationCode(user);
        smtpService.SendEmailVerificationRequest(user, code.Code);

        return new ApiOkResponse();
    }

    [ApiV3Endpoint("users/me", HttpMethods.Delete), MinimumRole(GameUserRole.Restricted)]
    [DocSummary("Deletes your own account. This action is non-reversible.")]
    public ApiOkResponse DeleteMyAccount(RequestContext context, GameUser user, GameDatabaseContext database)
    {
        database.DeleteUser(user);
        return new ApiOkResponse();
    }
}