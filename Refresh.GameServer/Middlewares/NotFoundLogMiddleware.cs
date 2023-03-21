using System.Net;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Endpoints.Middlewares;

namespace Refresh.GameServer.Middlewares;

public class NotFoundLogMiddleware : IMiddleware
{
    private const string EndpointFile = "unimplementedEndpoints.txt";
    private readonly List<string> _unimplementedEndpoints;

    public NotFoundLogMiddleware()
    {
        if (!File.Exists(EndpointFile)) {
            this._unimplementedEndpoints = new List<string>();
            return;
        }

        this._unimplementedEndpoints = File.ReadAllLines(EndpointFile).ToList();
    }
    
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        next(); // Handle the request so we can get the ResponseCode

        if (context.ResponseCode != HttpStatusCode.NotFound) return;
        
        if(!File.Exists(EndpointFile)) File.WriteAllText(EndpointFile, string.Empty);
        if (this._unimplementedEndpoints.Any(e => e.Split('?')[0] == context.Uri.AbsolutePath)) return;

        this._unimplementedEndpoints.Add(context.Uri.PathAndQuery);
        File.WriteAllLines(EndpointFile, this._unimplementedEndpoints);
    }
}