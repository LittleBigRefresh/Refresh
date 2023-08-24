using Bunkum.HttpServer.Storage;
using NotEnoughLogs;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Workers;

public class ExpiredObjectWorker : IWorker
{
    public int WorkInterval => 300_000; // 5 minutes
    public bool DoWork(LoggerContainer<RefreshContext> logger, IDataStore dataStore, GameDatabaseContext database)
    {
        bool removed = false;
        foreach (QueuedRegistration registration in database.GetAllQueuedRegistrations().Items)
        {
            if (!database.IsRegistrationExpired(registration)) continue;
            
            database.RemoveRegistrationFromQueue(registration);
            logger.LogInfo(RefreshContext.Worker, $"Removed {registration.Username}'s queued registration since it has expired");
            removed = true;
        }
        
        foreach (EmailVerificationCode code in database.GetAllVerificationCodes().Items)
        {
            if (!database.IsVerificationCodeExpired(code)) continue;
            
            database.RemoveEmailVerificationCode(code);
            logger.LogInfo(RefreshContext.Worker, $"Removed {code.User}'s verification code since it has expired");
            removed = true;
        }

        return removed;
    }
}