using System.Xml.Serialization;
using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints;

public class AuthenticationEndpoints : EndpointGroup
{
    [GameEndpoint("login", Method.Post, ContentType.Xml)]
    public LoginResponse Authenticate(RequestContext context)
    {
        return new LoginResponse
        {
            Token = "yeah",
            ServerBrand = "Refresh",
        };
    }
}

[XmlRoot("loginResult")]
public struct LoginResponse
{
    [XmlElement("authTicket")]
    public string Token { get; set; }
    
    [XmlElement("lbpEnvVer")]
    public string ServerBrand { get; set; }
}