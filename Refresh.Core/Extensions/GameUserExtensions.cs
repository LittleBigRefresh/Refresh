using Refresh.Core.Configuration;
using Refresh.Database.Models.Users;

namespace Refresh.Core.Extensions;

public static class GameUserExtensions
{
    public static bool IsWriteBlocked(this GameUser user, GameServerConfig config)
    {
        // Admins may always bypass this
        if (user.Role == GameUserRole.Admin) return false;
        
        // Restricted and Banned may not upload/edit any UGC, they also have no role perms because unnecessary
        else if (user.Role <= GameUserRole.Restricted) return true;
        
        // Determine based on role perms
        else return GetRolePermissionsForUser(user, config).ReadOnlyMode;
    }

    public static bool MayModifyUser(this GameUser user, GameUser targetUser)
    {
        // Users who are not at least a moderator may not update anyone else.
        if (user.Role < GameUserRole.Moderator)
            return false;

        // Only admins may modify everyone, even other admins. Moderators may not modify other moderators and no admins either.
        if (user.Role < GameUserRole.Admin && targetUser.Role >= GameUserRole.Moderator)
            return false;

        return true;
    }

    public static RolePermissions GetRolePermissionsForUser(this GameUser user, GameServerConfig config)
    {
        return user.Role switch
        {
            >= GameUserRole.Trusted => config.TrustedUserPermissions,
            GameUserRole.User => config.NormalUserPermissions,
            GameUserRole.NewUser => config.NewUserPermissions,
            _ => RolePermissions.FromRestrictedUser,
        };
    }
}