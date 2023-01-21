using System.Net;
using System.Xml.Serialization;
using NPTicket;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints.Handshake;

public class AuthenticationEndpoints : EndpointGroup
{
    [GameEndpoint("login", Method.Post, ContentType.Xml)]
    [NullStatusCode(HttpStatusCode.Forbidden)]
    [Authentication(false)]
    public LoginResponse? Authenticate(RequestContext context, RealmDatabaseContext database, Stream body)
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

        GameUser? user = database.GetUser(ticket.Username);
        user ??= database.CreateUser(ticket.Username);

        Token token = database.GenerateTokenForUser(user);
        
        return new LoginResponse
        {
            TokenData = "\r\nAuthorization: " + token.TokenData,
            ServerBrand = "Refresh",
        };
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