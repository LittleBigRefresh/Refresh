using System.Net;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints;

public class LicenseEndpoint : GameServerEndpoint
{
    protected override string GameRoute => "eula";
    
    public override Response GetResponse(HttpListenerContext context)
    {
        return new Response("Welcome to Refresh!");
    }
}