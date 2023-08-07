using System.Net;
using System.Xml.Serialization;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using NPTicket;
using NPTicket.Verification;
using NPTicket.Verification.Keys;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Verification;

namespace Refresh.GameServer.Endpoints.Game.Handshake;

public class AuthenticationEndpoints : EndpointGroup
{
    [GameEndpoint("login", Method.Post, ContentType.Xml), AllowDuringMaintenance]
    [NullStatusCode(Forbidden)]
    [Authentication(false)]
    public LoginResponse? Authenticate(RequestContext context, GameDatabaseContext database, Stream body, GameServerConfig config)
    {
        Ticket ticket;
        try
        {
            ticket = Ticket.ReadFromStream(body);
        }
        catch(Exception e)
        {
            context.Logger.LogWarning(BunkumContext.Authentication, "Could not read ticket: " + e);
            return null;
        }
        
        TokenPlatform? platform = ticket.IssuerId switch
        {
            0x100 => TokenPlatform.PS3,
            0x33333333 => TokenPlatform.RPCS3,
            _ => null,
        };
        
        GameUser? user = database.GetUserByUsername(ticket.Username);
        if (user == null)
        {
            if (config is { UseTicketVerification: false, AllowUsersToUseIpAuthentication: false })
            {
                // if no authentication methods are enabled, then just create a new user
                user ??= database.CreateUser(ticket.Username);
            }
            else return null;
        }
        else if(user.Role == GameUserRole.Banned)
            return null;

        if (config.MaintenanceMode && user.Role != GameUserRole.Admin)
            return null;

        bool ticketVerified = false;
        if (config.UseTicketVerification)
        {
            ticketVerified = VerifyTicket(context, (MemoryStream)body, ticket);
            if (!ticketVerified)
            {
                SendVerificationFailureNotification(database, user, config);
                if(!config.AllowUsersToUseIpAuthentication) return null;
            }
        }
        
        if (config.AllowUsersToUseIpAuthentication && !ticketVerified)
        {
            if (!HandleIpAuthentication(context, user, database, !config.UseTicketVerification)) return null;
        }

        TokenGame? game = TokenGameUtility.FromTitleId(ticket.TitleId);

        if (platform == null)
        {
            database.AddLoginFailNotification("The server could not determine what platform you were trying to connect from.", user);
            context.Logger.LogWarning(BunkumContext.Authentication, $"Could not determine platform from ticket.\n" +
                                                                    $"Platform: {ticket.IssuerId}");
            return null;
        }

        if (game == null)
        {
            database.AddLoginFailNotification("The server could not determine what game you were trying to connect from.", user);
            context.Logger.LogWarning(BunkumContext.Authentication, $"Could not determine game from ticket.\n" +
                                                                    $"Platform: {ticket.TitleId}");
            return null;
        }

        if (game == TokenGame.LittleBigPlanetVita && platform == TokenPlatform.PS3) platform = TokenPlatform.Vita;

        Token token = database.GenerateTokenForUser(user, TokenType.Game, game.Value, platform.Value,14400); // 4 hours

        return new LoginResponse
        {
            TokenData = "MM_AUTH=" + token.TokenData,
            ServerBrand = "Refresh",
        };
    }

    private static bool VerifyTicket(RequestContext context, MemoryStream body, Ticket ticket)
    {
        ITicketSigningKey signingKey;

        // Determine the correct key to use
        if (ticket.IssuerId == 0x33333333)
        {
            context.Logger.LogDebug(BunkumContext.Authentication, "Using RPCN ticket key");
            signingKey = RpcnSigningKey.Instance;
        }
        else
        {
            context.Logger.LogDebug(BunkumContext.Authentication, "Using PSN LBP ticket key");
            signingKey = LbpSigningKey.Instance;
        }
            
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
        
        string address = ((IPEndPoint)context.RemoteEndpoint).Address.ToString();
        if (address == user.CurrentVerifiedIp) return true;

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

    /// <summary>
    /// Called by the game when it exits cleanly.
    /// </summary>
    [GameEndpoint("goodbye", Method.Post, ContentType.Xml)]
    public Response RevokeThisToken(RequestContext context, GameDatabaseContext database, GameUser user)
    {
        string? token = context.Cookies["MM_AUTH"];
        
        // we shouldn't ever hit this but handle it anyways
        if (token == null) 
            return new Response("Token was somehow null", ContentType.Plaintext, InternalServerError);

        bool result = database.RevokeTokenByTokenData(token, TokenType.Game);

        if (!result)
            return Unauthorized;

        return OK;
    }
}

[XmlRoot("loginResult")]
public struct LoginResponse
{
    [XmlElement("authTicket")]
    public string TokenData { get; set; }
    
    [XmlElement("lbpEnvVer")]
    public string ServerBrand { get; set; }
}