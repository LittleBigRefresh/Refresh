using Bunkum.Listener.Request;
using Bunkum.Core.Endpoints.Middlewares;
using Refresh.Interfaces.Game;
using Bunkum.Protocols.Http;
using NotEnoughLogs;

namespace Refresh.GameServer.Middlewares;

/// <summary>
/// If the request was done by LBP PSP to one of the endpoints that start with <see cref="GameEndpointAttribute.BaseRoute"/>
/// + any prefix in RequiredRoutePrefixes, force its status code to be OK, even if it's not actually OK.
/// This way, LBP PSP won't softlock if a grief report upload fails, and it will hopefully not forcefully log out
/// if it fails to upload downloaded level stuff (scores achieved/tags given/ratins given/play counts while offline).
/// </summary>
public class PspStatusOverwriteMiddleware : IMiddleware
{
    public PspStatusOverwriteMiddleware(Logger logger)
    {
        this._logger = logger;
    }

    private Logger _logger;

    private static readonly string[] _requiredRoutePrefixes = 
    [
        // Trailing slash is enough, probably don't need to do extra effort to see whether
        // slot ID is actually a number
        "play/developer/",
        "play/user/",
        "favourite/slot/",
        "unfavourite/slot/",
        "favourite/user/",
        "unfavourite/user/",
        "scoreboard/developer/",
        "scoreboard/user/",
        // no need for developer slot type, since it will probably never happen on unmodded clients
        "tag/user/",
        "rate/user/",
        "grief",
    ];

    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        // Execute after everything else
        next();

        // No need for anything if response is already OK, or if it's Forbidden (since the user already isn't authenticated
        // in that case)
        if (context.ResponseCode == OK || context.ResponseCode == Forbidden)
        {
            return;
        }

        // Don't do anything if game isn't PSP. No need for token since this is just a "visual" change,
        // so hacking the user agent won't do much here.
        if (context.RequestHeaders.Get("User-Agent") != "LBPPSP CLIENT")
        {
            return;
        }

        // For now, all endpoints we have to do this for happen to be POST endpoints
        if (context.Method != HttpProtocolMethods.Post)
        {
            return;
        }

        // Incase this somehow isn't a (valid) game endpoint request
        string fullRoute = context.Uri.AbsolutePath;
        if (fullRoute.Length <= GameEndpointAttribute.BaseRoute.Length || !fullRoute.StartsWith(GameEndpointAttribute.BaseRoute))
        {
            return;
        }

        // Remove base from endpoint, and compare with our set prefixes
        string endpointRoute = fullRoute[GameEndpointAttribute.BaseRoute.Length..];
        foreach (string prefix in _requiredRoutePrefixes)
        {
            if (endpointRoute.StartsWith(prefix))
            {
                context.ResponseCode = OK; // overwrite if found, and stop iterating
                break;
            }
        }
    }
}