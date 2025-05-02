using Refresh.GameServer.Configuration;
using Refresh.Database.Models.Users;

namespace Refresh.GameServer.Extensions;

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