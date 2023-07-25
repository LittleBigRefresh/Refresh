using System.Xml.Serialization;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using NPTicket;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game.Handshake;

public class AuthenticationEndpoints : EndpointGroup
{
    [GameEndpoint("login", Method.Post, ContentType.Xml)]
    [NullStatusCode(Forbidden)]
    [Authentication(false)]
    public LoginResponse? Authenticate(RequestContext context, GameDatabaseContext database, Stream body)
    {
        Ticket ticket;
        try
        {
            ticket = Ticket.FromStream(body);
        }
        catch(Exception e)
        {
            context.Logger.LogWarning(BunkumContext.Authentication, "Could not read ticket: " + e);
            return null;
        }
        
        GameUser? user = database.GetUserByUsername(ticket.Username);
        user ??= database.CreateUser(ticket.Username);

        TokenPlatform? platform = ticket.IssuerId switch
        {
            0x100 => TokenPlatform.PS3,
            0x33333333 => TokenPlatform.RPCS3,
            _ => null,
        };

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