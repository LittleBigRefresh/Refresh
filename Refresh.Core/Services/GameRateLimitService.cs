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
    protected readonly EndpointRateLimiter RateLimiter;
    protected readonly GameAuthenticationService AuthService;

    public GameRateLimitService(Logger logger, TimeProviderService timeService, GameAuthenticationService authService, EndpointRateLimitConfig config) 
        : this(logger, new(timeService.TimeProvider, logger, config), authService) {}

    public GameRateLimitService(Logger logger, EndpointRateLimiter rateLimiter, GameAuthenticationService authService) : base(logger)
    {
        this.RateLimiter = rateLimiter;
        this.AuthService = authService;
    }

    public override Response? OnRequestHandled(ListenerContext context, MethodInfo method, Lazy<IDatabaseContext> database)
    {
        Token? token = this.AuthService.AuthenticateToken(context, database);

        bool violated = false;

        if (token != null)
            violated = this.RateLimiter.UserViolatesRateLimit(context, method, token.User);
        else
            violated = this.RateLimiter.RemoteEndpointViolatesRateLimit(context, method);

        if (violated) return new Response("You have been rate-limited.", ContentType.Plaintext, TooManyRequests);
        return null;
    }
}