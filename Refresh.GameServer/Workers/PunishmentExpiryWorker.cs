using Bunkum.Core.Storage;
using NotEnoughLogs;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Workers;

/// <summary>
/// A worker that checks all users for bans/restrictions, then removes them if expired.
/// </summary>
public class PunishmentExpiryWorker : IWorker
{
    public int WorkInterval => 60_000; // 1 minute

    public void DoWork(Logger logger, IDataStore dataStore, GameDatabaseContext database)
    {
        DatabaseList<GameUser> bannedUsers = database.GetAllUsersWithRole(GameUserRole.Banned);
        DatabaseList<GameUser> restrictedUsers = database.GetAllUsersWithRole(GameUserRole.Restricted);

        foreach (GameUser user in bannedUsers.Items)
        {
            if (database.IsUserBanned(user)) continue;
            
            logger.LogInfo(RefreshContext.Worker, $"Unbanned {user.Username} since their punishment has expired");
            database.SetUserRole(user, GameUserRole.User);
        }

        foreach (GameUser user in restrictedUsers.Items)
        {
            if (database.IsUserRestricted(user)) continue;
            
            logger.LogInfo(RefreshContext.Worker, $"Unrestricted {user.Username} since their punishment has expired");
            database.SetUserRole(user, GameUserRole.User);
        }
    }
}