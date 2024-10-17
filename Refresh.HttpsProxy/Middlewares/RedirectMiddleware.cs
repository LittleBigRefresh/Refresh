using System.Net;
using Bunkum.Core.Database;
using Bunkum.Core.Endpoints.Middlewares;
using Bunkum.Listener.Request;
using Refresh.Common.Extensions;
using Refresh.HttpsProxy.Config;

namespace Refresh.HttpsProxy.Middlewares;

public class RedirectMiddleware(ProxyConfig config) : IMiddleware
{
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        UriBuilder uri = new(config.TargetServerUrl)
        {
            Path = context.Uri.AbsolutePath,
        };

        context.Query["force_ps3_digest"] = config.Ps3DigestIndex.ToString();
        context.Query["force_ps4_digest"] = config.Ps4DigestIndex.ToString();

        uri.Query = context.Query.ToQueryString();

        context.ResponseCode = HttpStatusCode.TemporaryRedirect;
        context.ResponseHeaders["Location"] = uri.ToString();
    }
}