using System.Net;
using JetBrains.Annotations;
using Refresh.HttpServer.Responses;

namespace Refresh.HttpServer.Endpoints;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class Endpoint
{
    public abstract string Route { get; }

    [Pure]
    public abstract Response GetResponse(HttpListenerContext context);
}