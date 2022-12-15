using System.Xml.Serialization;
using NPTicket;
using Refresh.GameServer.Database;
using Refresh.GameServer.Database.Types;
using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints;

public class AuthenticationEndpoints : EndpointGroup
{
    [GameEndpoint("login", Method.Post, ContentType.Xml)]
    [Authentication(false)]
    public LoginResponse Authenticate(RequestContext context, RealmDatabaseContext database, Stream body)
    {
        Ticket ticket = Ticket.FromStream(body);
        
        GameUser? user = database.GetUser(ticket.Username);
        user ??= database.CreateUser(ticket.Username);

        Token token = database.GenerateTokenForUser(user);
        
        return new LoginResponse
        {
            TokenData = "\r\nAuthorization: " + token.TokenData,
            ServerBrand = "Refresh",
        };
    }

    private static readonly Lazy<string?> NetworkSettingsFile
        = new Lazy<string?>(() => 
        {
            string path = Path.Combine(Environment.CurrentDirectory, "network_settings.nws");

            return File.Exists(path) ? File.ReadAllText(path) : null;
        });
    
    [GameEndpoint("network_settings.nws")]
    public string? NetworkSettings(RequestContext context) {
        bool created = NetworkSettingsFile.IsValueCreated;
        
        string? networkSettings = NetworkSettingsFile.Value;
        
        if(!created && networkSettings == null)
            context.Logger.LogWarning(RefreshContext.Request, "network_settings.nws file is missing!");
        
        return networkSettings;
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