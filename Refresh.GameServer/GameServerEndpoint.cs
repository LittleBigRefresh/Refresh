using System.Net;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer;

public abstract class GameServerEndpoint : Endpoint
{
    public override string Route => "/LITTLEBIGPLANETPS3_XML/" + this.GameRoute;
    protected abstract string GameRoute { get; }
}