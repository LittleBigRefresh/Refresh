using System.Reflection;
using Bunkum.Core;
using Bunkum.Core.Authentication;
using Bunkum.Core.Database;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Core.Services;
using Bunkum.Listener.Protocol;
using Bunkum.Listener.Request;
using NotEnoughLogs;
using Refresh.Database.Models.Authentication;

namespace Refresh.Core.Services;

// Referenced from https://github.com/PlanetBunkum/Bunkum/blob/main/Bunkum.Core/Services/AuthenticationService.cs
// purposefully a less optimized implementation (as in it doesn't cache the token)
public class GameAuthenticationService : Service
{
    private readonly IAuthenticationProvider<Token> _provider;

    public GameAuthenticationService(Logger logger, IAuthenticationProvider<Token> provider) : base(logger)
    {
        this._provider = provider;
    }

    public override Response? OnRequestHandled(ListenerContext context, MethodInfo method, Lazy<IDatabaseContext> database)
    {
        if (!(method.GetCustomAttribute<AuthenticationAttribute>()?.Required ?? true)) return null;
        
        if (this.AuthenticateToken(context, database) == null)
            return new Response("Not authenticated", ContentType.Plaintext, Forbidden);

        return null;
    }

    public override object? AddParameterToEndpoint(ListenerContext context, BunkumParameterInfo parameter, Lazy<IDatabaseContext> database)
    {
        if (ParameterBasedFrom<IToken<IUser>>(parameter))
        {
            return this.AuthenticateToken(context, database);
        }

        if (ParameterBasedFrom<IUser>(parameter))
        {
            IToken<IUser>? token = this.AuthenticateToken(context, database);
            if (token != null) return token.User;
        }
        
        return null;
    }

    public Token? AuthenticateToken(ListenerContext context, Lazy<IDatabaseContext> database)
    {
        return this._provider.AuthenticateToken(context, database);
    }
}