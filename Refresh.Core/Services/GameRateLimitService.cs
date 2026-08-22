using System.Reflection;
using Bunkum.Core.Database;
using Bunkum.Core.Responses;
using Bunkum.Core.Services;
using Bunkum.Listener.Protocol;
using Bunkum.Listener.Request;
using NotEnoughLogs;
using Refresh.Core.RateLimits.EndpointRateLimiting;
using Refresh.Database.Models.Authentication;
using Refresh.Core.Configuration;

namespace Refresh.Core.Services;

// Referenced from https://github.com/PlanetBunkum/Bunkum/blob/main/Bunkum.Core/Services/RateLimitService.cs
public class GameRateLimitService : Service
{
    private readonly EndpointRateLimiter _rateLimiter;
    private readonly GameAuthenticationService _authService;

    internal GameRateLimitService(Logger logger, TimeProviderService timeService, GameAuthenticationService authService, EndpointRateLimitConfig config) : base(logger)
    {
        this._rateLimiter = new(timeService.TimeProvider, logger, config.Buckets);
        this._authService = authService;
    }

    public override Response? OnRequestHandled(ListenerContext context, MethodInfo method, Lazy<IDatabaseContext> database)
    {
        Token? token = this._authService.AuthenticateToken(context, database);

        bool violated = false;

        if (token != null)
            violated = this._rateLimiter.UserViolatesRateLimit(context, method, token.User);
        else
            violated = this._rateLimiter.RemoteEndpointViolatesRateLimit(context, method);

        if (violated) return new Response("You have been rate-limited.", ContentType.Plaintext, TooManyRequests);
        return null;
    }
}