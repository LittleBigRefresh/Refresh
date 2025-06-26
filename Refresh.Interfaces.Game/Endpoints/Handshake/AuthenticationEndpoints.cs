using System.Xml.Serialization;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.RateLimit;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using NPTicket;
using NPTicket.Verification;
using NPTicket.Verification.Keys;
using Refresh.Common.Time;
using Refresh.Common.Verification;
using Refresh.Core;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Core.Services;
using Refresh.Core.Types.Matching;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;

namespace Refresh.Interfaces.Game.Endpoints.Handshake;

public class AuthenticationEndpoints : EndpointGroup
{
    [GameEndpoint("login", HttpMethods.Post, ContentType.Xml), Authentication(false), AllowDuringMaintenance]
    [NullStatusCode(Forbidden)]
    [RateLimitSettings(300, 10, 300, "auth")]
    [MinimumRole(GameUserRole.Restricted)]
    public object? Authenticate(RequestContext context,
        GameDatabaseContext database,
        Stream body,
        GameServerConfig config,
        IntegrationConfig integrationConfig,
        SmtpService smtpService,
        IDateTimeProvider timeProvider)
    {
        if (!config.PermitAllLogins)
        {
            return null;
        }
        
        Ticket ticket;
        try
        {
            ticket = Ticket.ReadFromStream(body);
        }
        catch(Exception e)
        {
            context.Logger.LogWarning(BunkumCategory.Authentication, "Could not read ticket: " + e);
            return null;
        }

        string ipAddress = context.RemoteIp();
        
        TokenPlatform? platform = ticket.DeterminePlatform();
        
        GameUser? user = database.GetUserByUsername(ticket.Username);
        if (user == null)
        {
            if (config.RequireGameLoginToRegister)
            {
                // look for a registration, then use that to create a user
                QueuedRegistration? registration = database.GetQueuedRegistrationByUsername(ticket.Username);
                if (registration == null)
                {
                    context.Logger.LogWarning(BunkumCategory.Authentication, $"Rejecting {ticket.Username}'s login because there was no matching queued registration");
                    return null;
                }
                
                user = database.CreateUserFromQueuedRegistration(registration, platform);
                
                if (integrationConfig.SmtpEnabled)
                {
                    EmailVerificationCode code = database.CreateEmailVerificationCode(user);
                    smtpService.SendEmailVerificationRequest(user, code.Code);

                    context.Logger.LogInfo(BunkumCategory.Authentication, $"Telling user {user.Username} to verify their email");
                    database.AddNotification("Verify your email",
                        "Your account has been created, but you still need to verify your e-mail." +
                           "Please check your email for a verification code, and verify it in settings." +
                           "If you do not see an email verification code, try resending the email or checking your spam folder.",
                        user, "envelope");
                }
                else
                {
                    // if smtp isn't enabled just mark the user's email as verified
                    database.VerifyUserEmail(user);
                }
            }
            else
            {
                context.Logger.LogWarning(BunkumCategory.Authentication, $"Rejecting {ticket.Username}'s login because there was no matching username");
                return null;
            }
        }
        else if (user.Role == GameUserRole.Banned)
        {
            context.Logger.LogWarning(BunkumCategory.Authentication, $"Rejecting {user}'s login because they are banned");
            return null;
        }

        if (config.MaintenanceMode && user.Role != GameUserRole.Admin)
        {
            context.Logger.LogWarning(BunkumCategory.Authentication, $"Rejecting {user}'s login because server is in maintenance mode");
            return null;
        }
        
        if (platform == null)
        {
            database.AddLoginFailNotification("The server could not determine what platform you were trying to connect from.", user);
            context.Logger.LogWarning(BunkumCategory.Authentication, $"Could not determine platform from ticket. " +
                                                                     $"IssuerID: {ticket.IssuerId}, SignatureIdentifier: {ticket.SignatureIdentifier}");
            return null;
        }
        
        if (platform is TokenPlatform.PS3 or TokenPlatform.PSP or TokenPlatform.Vita && !config.PermitPsnLogin)
        {
            context.Logger.LogWarning(BunkumCategory.Authentication, $"Rejecting {user}'s login because PSN login is forbidden");
            return null;
        }

        if (platform is TokenPlatform.RPCS3 && !config.PermitRpcnLogin)
        {
            context.Logger.LogWarning(BunkumCategory.Authentication, $"Rejecting {user}'s login because RPCN login is forbidden");
            return null;
        }

        if (platform is TokenPlatform.Website)
        {
            context.Logger.LogWarning(BunkumCategory.Authentication, $"Rejecting {user}'s login because a web token was used");
            return null;
        }

        bool ticketVerified = false;
        if (config.UseTicketVerification)
        {
            if ((platform is TokenPlatform.PS3 or TokenPlatform.Vita or TokenPlatform.PSP && !user.PsnAuthenticationAllowed) ||
                (platform is TokenPlatform.RPCS3 && !user.RpcnAuthenticationAllowed))
            {
                context.Logger.LogWarning(BunkumCategory.Authentication, $"Rejecting {user}'s login because their platform ({platform}) is not allowed in user settings");
                SendPlatformNotAllowedNotification(database, user, platform.Value);
                return null;
            }
            
            ticketVerified = VerifyTicket(context, (MemoryStream)body, ticket, platform.Value, timeProvider);
            if (!ticketVerified)
            {
                SendVerificationFailureNotification(database, user, config);
                if (!config.AllowUsersToUseIpAuthentication)
                {
                    context.Logger.LogWarning(BunkumCategory.Authentication, $"Rejecting {user}'s login because their ticket could not be verified");
                    return null;
                }
            }
        }
        
        if (config.AllowUsersToUseIpAuthentication && !ticketVerified)
        {
            if (!HandleIpAuthentication(context, user, database, !config.UseTicketVerification))
            {
                context.Logger.LogWarning(BunkumCategory.Authentication, $"Rejecting {user}'s login from {ipAddress} because the IP was not whitelisted");
                SendIpNotVerifiedNotification(database, user, ipAddress);
                return null;
            }
        }

        TokenGame? game = ticket.DetermineGame(context);

        if (game == null)
        {
            database.AddLoginFailNotification($"The server could not determine what game you were trying to connect from. Give this ID to a developer: {ticket.TitleId}", user);
            context.Logger.LogWarning(BunkumCategory.Authentication, $"Could not determine game from ticket.\n" +
                                                                    $"Missing TitleID: {ticket.TitleId}");
            return null;
        }

        if (game == TokenGame.LittleBigPlanetVita && platform == TokenPlatform.PS3) platform = TokenPlatform.Vita;
        else if (game == TokenGame.LittleBigPlanetPSP && platform == TokenPlatform.PS3) platform = TokenPlatform.PSP;
        
        // Check if client-side security patches are present, if required.
        // PSP is invulnerable to most exploits as it does not support multiplayer nor scripting.
        if (config.EnforcePatchwork &&
            game != TokenGame.LittleBigPlanetPSP &&
            !context.IsPatchworkVersionValid(config.RequiredPatchworkMajorVersion, config.RequiredPatchworkMinorVersion))
        {
            database.AddLoginFailNotification("The server detected you are not using the latest version of Patchwork. Please update or install it.", user);
            context.Logger.LogWarning(BunkumCategory.Authentication, $"{ticket.Username}'s Patchwork version is invalid: {context.RequestHeaders["User-Agent"]}");
            return null;
        }
        
        // !!
        // Past this point, login is considered to be complete.
        // !!

        Token token = database.GenerateTokenForUser(user, TokenType.Game, game.Value, platform.Value, ipAddress, GameDatabaseContext.GameTokenExpirySeconds); // 4 hours

        // Clear the user's force match
        database.ClearForceMatch(user);
        
        // Mark the user as disconnected from the presence server
        database.SetUserPresenceAuthToken(user, null);

        // Automatically verify the ip in use, since this is now a trusted connection
        if (!database.IsIpVerified(user, ipAddress)) 
            database.AddVerifiedIp(user, ipAddress, timeProvider);
        
        // Avoid confusing users by sending them login failure notifications in-game
        database.ClearLoginFailNotificationsForUser(user);

        context.Logger.LogInfo(BunkumCategory.Authentication, $"{user} successfully logged in on {game} via {platform}");
        
        if (game == TokenGame.LittleBigPlanetPSP)
        {
            return new TicketLoginResponse
            {
                TokenData = "MM_AUTH=" + token.TokenData,
            };
        }

        return new FullLoginResponse
        {
            TokenData = "MM_AUTH=" + token.TokenData,
            ServerBrand = $"{config.InstanceName} (Refresh {VersionInformation.Version})",
            TitleStorageUrl = config.GameConfigStorageUrl,
        };
    }

    private static bool VerifyTicket(RequestContext context, MemoryStream body, Ticket ticket, TokenPlatform platform, IDateTimeProvider timeProvider)
    {
        ITicketSigningKey signingKey;

        switch (platform)
        {
            // Determine the correct key to use
            case TokenPlatform.RPCS3:
                context.Logger.LogDebug(BunkumCategory.Authentication, "Using RPCN ticket key");
                signingKey = RpcnSigningKey.Instance;
                break;
            case TokenPlatform.PS3:
            case TokenPlatform.Vita:
            case TokenPlatform.PSP:
                context.Logger.LogDebug(BunkumCategory.Authentication, "Using PSN LBP ticket key");
                signingKey = LbpSigningKey.Instance;
                break;
            case TokenPlatform.Website:
            default:
                throw new ArgumentOutOfRangeException(nameof(platform));
        }

        // Dont allow use of expired tickets
        if (timeProvider.Now > ticket.ExpiryDate)
            return false;
            
        // Pass this information into a new ticket verifier
        // TODO: make this into a service?
        TicketVerifier verifier = new(body.ToArray(), ticket, signingKey);
        return verifier.IsTicketValid();
    }

    private static bool HandleIpAuthentication(RequestContext context, GameUser user, GameDatabaseContext database, bool notify)
    {
        if (user.AllowIpAuthentication == false)
        {
            if (notify)
            {
                database.AddLoginFailNotification(
                    "This server requires IP authentication to be enabled, but your account doesn't have this enabled. " +
                    "If this was you, enable IP authentication in settings.", user);
            }
            return false;
        }
        
        string address = context.RemoteIp();
        
        if (database.IsIpVerified(user, address)) 
            return true;

        database.AddIpVerificationRequest(user, address);
        return false;
    }

    private static void SendVerificationFailureNotification(GameDatabaseContext database, GameUser user, GameServerConfig config)
    {
        const string failHeader = "The ticket could not be verified.";
        string failReason = failHeader + '\n';

        if (config.AllowUsersToUseIpAuthentication)
        {
            if (!user.AllowIpAuthentication)
            {
                failReason +=
                    "This server allows IP authentication to be enabled, but your account doesn't have this enabled ." +
                    "If this was you, enable IP authentication in settings.";
            }
        }
        else
        {
            failReason += "This server does not allow IP authentication to be enabled, " +
                          "so please make sure you are using a supported ticket server (e.g. PSN or RPCN.)\n" +
                          "For more information, contact the server owner.";
        }
                    
        database.AddLoginFailNotification(failReason, user);
    }

    private static void SendPlatformNotAllowedNotification(GameDatabaseContext database, GameUser user, TokenPlatform platform)
    {
        database.AddLoginFailNotification($"An authentication attempt was made from {platform}, " +
                                          $"but the respective option for it was disabled. To allow authentication from " +
                                          $"{platform}, enable '{platform} Authentication' in settings.", user);
    }

    private static void SendIpNotVerifiedNotification(GameDatabaseContext database, GameUser user, string ipAddress)
    {
        database.AddLoginFailNotification($"A login attempt was detected from IP '{ipAddress}'. " +
                                          "To authorize this IP, please verify it in your settings. " +
                                          "If this wasn't you, please reject the request.", user);
    }

    /// <summary>
    /// Called by the game when it exits cleanly.
    /// </summary>
    [GameEndpoint("goodbye", HttpMethods.Post, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public Response RevokeThisToken(RequestContext context, GameDatabaseContext database, Token token, GameUser user, MatchService matchService)
    {
        //If the user is the host of a room, remove that room
        GameRoom? room = matchService.RoomAccessor.GetRoomByUser(token.User, token.TokenPlatform, token.TokenGame);
        if (room != null && room.HostId.Id == token.User.UserId)
            matchService.RoomAccessor.RemoveRoom(room.RoomId);
        
        // Revoke the token
        database.RevokeToken(token);
        // Mark them as disconnected from the presence server
        database.SetUserPresenceAuthToken(user, null);
        
        context.Logger.LogInfo(BunkumCategory.Authentication, $"{user} logged out");
        return OK;
    }
}

[XmlRoot("loginResult")]
public struct FullLoginResponse
{
    [XmlElement("authTicket")]
    public string TokenData { get; set; }
    
    [XmlElement("lbpEnvVer")]
    public string ServerBrand { get; set; }
    
    [XmlElement("titleStorageURL")]
    public string TitleStorageUrl { get; set; }
}

[XmlRoot("authTicket")]
public struct TicketLoginResponse
{
    [XmlText]
    public string TokenData { get; set; }
}