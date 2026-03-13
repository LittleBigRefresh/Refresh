using System.Reflection;
using Bunkum.Core.Database;
using Bunkum.Core.RateLimit;
using Bunkum.Core.Responses;
using Bunkum.Core.Services;
using Bunkum.Listener.Protocol;
using Bunkum.Listener.Request;
using NotEnoughLogs;
using Refresh.Database.Models.Users;

namespace Refresh.Core.Services;

// Referenced from https://github.com/PlanetBunkum/Bunkum/blob/main/Bunkum.Core/Services/RateLimitService.cs
public class GameRateLimitService : Service
{
    private readonly IRateLimiter _rateLimiter;
    private readonly GameAuthenticationService _authService;

    internal GameRateLimitService(Logger logger, GameAuthenticationService authService, IRateLimiter rateLimiter) : base(logger)
    {
        this._rateLimiter = rateLimiter;
        this._authService = authService;
    }

    public override Response? OnRequestHandled(ListenerContext context, MethodInfo method, Lazy<IDatabaseContext> database)
    {
        GameUser? user = this._authService.AuthenticateToken(context, database)?.User;

        bool violated = false;

        if (user != null)
            violated = this._rateLimiter.UserViolatesRateLimit(context, method, user);
        else
            violated = this._rateLimiter.RemoteEndpointViolatesRateLimit(context, method);

        if (violated) return new Response("You have been rate-limited.", ContentType.Plaintext, TooManyRequests);
        return null;
    }
}