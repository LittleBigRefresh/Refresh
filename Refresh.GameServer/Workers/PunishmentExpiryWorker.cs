using Refresh.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Users;

namespace Refresh.GameServer.Workers;

/// <summary>
/// A worker that checks all users for bans/restrictions, then removes them if expired.
/// </summary>
public class PunishmentExpiryWorker : IWorker
{
    public int WorkInterval => 60_000; // 1 minute

    public void DoWork(DataContext context)
    {
        DatabaseList<GameUser> bannedUsers = context.Database.GetAllUsersWithRole(GameUserRole.Banned);
        DatabaseList<GameUser> restrictedUsers = context.Database.GetAllUsersWithRole(GameUserRole.Restricted);

        foreach (GameUser user in bannedUsers.Items)
        {
            if (context.Database.IsUserBanned(user)) continue;
            
            context.Logger.LogInfo(RefreshContext.Worker, $"Unbanned {user.Username} since their punishment has expired");
            context.Database.SetUserRole(user, GameUserRole.User);
        }

        foreach (GameUser user in restrictedUsers.Items)
        {
            if (context.Database.IsUserRestricted(user)) continue;
            
            context.Logger.LogInfo(RefreshContext.Worker, $"Unrestricted {user.Username} since their punishment has expired");
            context.Database.SetUserRole(user, GameUserRole.User);
        }
    }
}