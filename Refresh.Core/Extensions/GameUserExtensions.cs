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
}