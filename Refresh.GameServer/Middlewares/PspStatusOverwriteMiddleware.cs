using Bunkum.Listener.Request;
using Bunkum.Core.Endpoints.Middlewares;
using Refresh.Interfaces.Game;
using Bunkum.Protocols.Http;
using NotEnoughLogs;
using Refresh.Common;
using Bunkum.Core;

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

    // Route prefix -> response body content to write, if there needs to be any
    private static readonly Dictionary<string, string> _requiredRoutePrefixes = new()
    {
        // Trailing slash is enough, probably don't need to do extra effort to see whether
        // slot ID is actually a number
        {"play/developer/", ""},
        {"play/user/", ""},
        {"favourite/slot/", ""},
        {"unfavourite/slot/", ""},
        {"favourite/user/", ""},
        {"unfavourite/user/", ""},
        // Incase the game also expects an actual score list, return an empty one
        {"scoreboard/developer/", "<scoreboardSegment />"},
        {"scoreboard/user/", "<scoreboardSegment />"},
        // no need for developer slot type, since it will probably never happen on unmodded clients
        {"tag/user/", ""},
        {"rate/user/", ""},
        {"grief", ""},
    };

    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        // Execute after everything else
        next();

        // No need for anything if response is already OK, or if it's Forbidden (since the user already isn't authenticated
        // in that case)
        if (context.ResponseCode == OK || context.ResponseCode == Forbidden)
        {
            this._logger.LogDebug(BunkumCategory.Middleware, $"PSP status overwrite: status is {context.ResponseCode} (not OK or Forbidden), no need to overwrite");
            return;
        }

        // Don't do anything if game isn't PSP. No need for token since this is just a "visual" change,
        // so hacking the user agent won't do much here.
        if (context.RequestHeaders.Get("User-Agent") != "LBPPSP CLIENT")
        {
            this._logger.LogDebug(BunkumCategory.Middleware, $"PSP status overwrite: user agent is not PSP ({context.RequestHeaders.Get("User-Agent")}), no need to overwrite");
            return;
        }

        // For now, all endpoints we have to do this for happen to be POST endpoints
        if (context.Method != HttpProtocolMethods.Post)
        {
            this._logger.LogDebug(BunkumCategory.Middleware, $"PSP status overwrite: method is {context.Method} (not POST), no need to overwrite");
            return;
        }

        // Incase this somehow isn't a (valid) game endpoint request
        string fullRoute = context.Uri.AbsolutePath;
        if (fullRoute.Length <= GameEndpointAttribute.BaseRoute.Length || !fullRoute.StartsWith(GameEndpointAttribute.BaseRoute))
        {
            this._logger.LogDebug(BunkumCategory.Middleware, $"Full route {fullRoute} doesn't start with /lbp/ or is too short");
            return;
        }

        // Remove base from endpoint, and compare with our set prefixes
        string endpointRoute = fullRoute[GameEndpointAttribute.BaseRoute.Length..];
        foreach (KeyValuePair<string, string> prefix in _requiredRoutePrefixes)
        {
            if (endpointRoute.StartsWith(prefix.Key))
            {
                // If we should also write something into the body, then do so.
                if (prefix.Value != "")
                {
                    context.ResponseStream.Position = 0;
                    context.ResponseStream.SetLength(0);
                    context.Write(prefix.Value);
                }

                this._logger.LogDebug(BunkumCategory.Middleware, $"Prefix {prefix} matches endpoint route {endpointRoute}, overwriting");
                context.ResponseCode = OK; // overwrite if found, and stop iterating
                break;
            }
            
            this._logger.LogDebug(BunkumCategory.Middleware, $"Prefix {prefix} doesnt match endpoint route {endpointRoute}, skipping");
        }
    }
}