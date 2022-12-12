using System.Net;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints;

public class AuthenticateEndpoint : GameServerEndpoint
{
    protected override string GameRoute => "login";

    public override Response GetResponse(HttpListenerContext context)
    {
        return new Response("<loginResult><authTicket>yeah</authTicket><lbpEnvVer>Refresh</lbpEnvVer></loginResult>", ContentType.Xml);
    }
}