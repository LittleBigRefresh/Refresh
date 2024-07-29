using System.Reflection;
using Bunkum.Listener.Request;
using Bunkum.Core.Database;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Core.Services;
using Bunkum.Protocols.Http;
using NotEnoughLogs;
using Refresh.GameServer.Configuration;
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
    private readonly GameServerConfig _config;
    
    internal RoleService(AuthenticationService authService, GameServerConfig config, Logger logger) : base(logger)
    {
        this._authService = authService;
        this._config = config;
    }

    private Response? OnNormalRequestHandled(ListenerContext context, MemberInfo method, Lazy<IDatabaseContext> database)
    {
        AuthenticationAttribute? authAttrib = method.GetCustomAttribute<AuthenticationAttribute>();
        if (!(authAttrib?.Required ?? true)) return null;
        
        MinimumRoleAttribute? roleAttrib = method.GetCustomAttribute<MinimumRoleAttribute>();
        GameUserRole minimumRole = roleAttrib?.MinimumRole ?? GameUserRole.User;

        GameUser? user = (GameUser?)this._authService.AuthenticateToken(context, database)?.User;
        if (user == null) return null; // Let AuthenticationProvider handle 401
        
        // if the user's role is lower than the minimum role for this endpoint, then return unauthorized
        if (user.Role < minimumRole)
        {
            this._authService.RemoveTokenFromCache();
            return Unauthorized;
        }

        RequireEmailVerifiedAttribute? emailAttrib = method.GetCustomAttribute<RequireEmailVerifiedAttribute>();
        if (emailAttrib != null && !user.EmailAddressVerified)
        {
            this._authService.RemoveTokenFromCache();
            return Unauthorized;
        }

        return null;
    }

    private Response? OnMaintenanceModeRequestHandled(ListenerContext context, MemberInfo method, Lazy<IDatabaseContext> database)
    {
        if (method.GetCustomAttribute<AllowDuringMaintenanceAttribute>() != null)
            return this.OnNormalRequestHandled(context, method, database);

        GameUser? user = (GameUser?)this._authService.AuthenticateToken(context, database)?.User;
        if (user == null) return Forbidden;

        // If user isn't an admin, then stop the request here, ignoring all
        if (user.Role != GameUserRole.Admin)
        {
            this._authService.RemoveTokenFromCache();
            return Forbidden;
        }
        

        return null;
    }

    public override Response? OnRequestHandled(ListenerContext context, MethodInfo method, Lazy<IDatabaseContext> database)
    {
        if (this._config.MaintenanceMode)
            return this.OnMaintenanceModeRequestHandled(context, method, database);
        
        return this.OnNormalRequestHandled(context, method, database);
    }
}