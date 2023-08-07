using Bunkum.HttpServer.Storage;
using NotEnoughLogs;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Workers;

/// <summary>
/// A worker that checks all users for bans, then removes them if expired.
/// </summary>
public class BanExpiryWorker : IWorker
{
    public int WorkInterval => 60_000;

    public bool DoWork(LoggerContainer<RefreshContext> logger, IDataStore dataStore, GameDatabaseContext database)
    {
        DatabaseList<GameUser> bannedUsers = database.GetAllUsersWithRole(GameUserRole.Banned);
        bool didUnban = false;
        foreach (GameUser user in bannedUsers.Items)
        {
            if (database.IsUserBanned(user)) continue;
            
            logger.LogInfo(RefreshContext.Worker, $"Unbanned {user.Username} since their ban expired");
            database.SetUserRole(user, GameUserRole.User);
            didUnban = true;
        }

        return didUnban;
    }
}