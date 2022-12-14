using System.Xml.Serialization;
using Refresh.GameServer.Database;
using Refresh.GameServer.Database.Types;
using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints;

public class AuthenticationEndpoints : EndpointGroup
{
    [GameEndpoint("login", Method.Post, ContentType.Xml)]
    public LoginResponse Authenticate(RequestContext context, RealmDatabaseContext database)
    {
        GameUser? user = database.GetUser("jvyden420");
        user ??= database.CreateUser("jvyden420");

        Token token = database.GenerateTokenForUser(user);
        
        return new LoginResponse
        {
            TokenData = token.TokenData,
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