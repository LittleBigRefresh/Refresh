using System.Reflection;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Bunkum.HttpServer.Services;
using NotEnoughLogs;
using Refresh.GameServer.Endpoints;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Services;

/// <summary>
/// A service that hooks into the AuthenticationService, adding extra checks for roles.
/// </summary>
public class RoleService : Service
{
    private readonly AuthenticationService _authService;
    
    internal RoleService(AuthenticationService authService, LoggerContainer<BunkumContext> logger) : base(logger)
    {
        this._authService = authService;
    }

    public override Response? OnRequestHandled(ListenerContext context, MethodInfo method, Lazy<IDatabaseContext> database)
    {
        AuthenticationAttribute? authAttrib = method.GetCustomAttribute<AuthenticationAttribute>();
        if (!(authAttrib?.Required ?? true)) return null;
        
        MinimumRoleAttribute? roleAttrib = method.GetCustomAttribute<MinimumRoleAttribute>();
        GameUserRole minimumRole = roleAttrib?.MinimumRole ?? GameUserRole.User;

        GameUser? user = (GameUser?)this._authService.AuthenticateUser(context, database);
        if (user == null) return null; // Let AuthenticationProvider handle 401
        
        // if the user's role is lower than the minimum role for this endpoint, then return unauthorized
        if (user.Role < minimumRole)
            return Unauthorized;

        return null;
    }
}