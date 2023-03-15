using System.Net;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Endpoints.Middlewares;
using Refresh.GameServer.Endpoints;

namespace Refresh.GameServer.Middlewares;

public class CrossOriginMiddleware : IMiddleware
{
    private static readonly List<string> _allowedMethods = new();

    static CrossOriginMiddleware()
    {
        foreach (Method method in Enum.GetValues<Method>())
        {
            if(method is Method.Options or Method.Invalid) continue;
            _allowedMethods.Add(method.ToString().ToUpperInvariant());
        }
    }
    
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        // Allow any origin for API
        // Mozilla says this is okay:
        //   "You can also configure a site to allow any site to access it by using the * wildcard. You should only use this for public APIs."
        // https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS/Errors/CORSMissingAllowOrigin#what_went_wrong
        if (context.Uri.AbsolutePath.StartsWith(ApiEndpointAttribute.BaseRoute))
        {
            context.ResponseHeaders.Add("Access-Control-Allow-Origin", "*");
            context.ResponseHeaders.Add("Access-Control-Allow-Headers", "Authorization");
            context.ResponseHeaders.Add("Access-Control-Allow-Methods", string.Join(", ", _allowedMethods));
            
            if (context.Method == Method.Options)
            {
                context.ResponseCode = HttpStatusCode.OK;
                return;
            }
        }
        
        next();
    }
}