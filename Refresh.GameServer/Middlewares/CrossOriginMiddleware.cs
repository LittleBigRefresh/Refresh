using Bunkum.CustomHttpListener.Parsing;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Endpoints.Middlewares;
using Refresh.GameServer.Endpoints;

namespace Refresh.GameServer.Middlewares;

public class CrossOriginMiddleware : IMiddleware
{
    private static readonly List<string> AllowedMethods = new();

    static CrossOriginMiddleware()
    {
        foreach (Method method in Enum.GetValues<Method>())
        {
            if(method is Method.Options or Method.Invalid) continue;
            AllowedMethods.Add(method.ToString().ToUpperInvariant());
        }
    }

    private static void AllowAnyOrigin(ListenerContext context)
    {
        context.ResponseHeaders.Add("Access-Control-Allow-Origin", "*");

        if (context.Method != Method.Options) return;
        
        context.ResponseHeaders.Add("Access-Control-Allow-Headers", "Authorization, Content-Type");
        context.ResponseHeaders.Add("Access-Control-Allow-Methods", string.Join(", ", AllowedMethods));
                
        context.ResponseCode = OK;
    }
    
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        // Allow any origin for API
        // Mozilla says this is okay:
        //   "You can also configure a site to allow any site to access it by using the * wildcard. You should only use this for public APIs."
        // https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS/Errors/CORSMissingAllowOrigin#what_went_wrong
        if (context.Uri.AbsolutePath.StartsWith(ApiV2EndpointAttribute.BaseRoute) ||
            context.Uri.AbsolutePath.StartsWith(ApiV3EndpointAttribute.BaseRoute))
        {
            AllowAnyOrigin(context);
            return;
        }
        
        next();
    }
}