using Refresh.Core.Configuration;
using Refresh.Database.Models.Users;

namespace Refresh.Core.Extensions;

public static class GameUserExtensions
{
    public static bool IsWriteBlocked(this GameUser user, GameServerConfig config)
    {
        if (config.ReadOnlyMode && user.Role != GameUserRole.Admin)
        {
            return user.Role < GameUserRole.Trusted || config.ReadonlyModeForTrustedUsers;
        }

        return false;
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
}