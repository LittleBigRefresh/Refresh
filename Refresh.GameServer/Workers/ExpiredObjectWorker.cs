using Bunkum.Core.Storage;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Workers;

public class ExpiredObjectWorker : IWorker
{
    public int WorkInterval => 300_000; // 5 minutes
    public void DoWork(Logger logger, IDataStore dataStore, GameDatabaseContext database)
    {
        foreach (QueuedRegistration registration in database.GetAllQueuedRegistrations().Items)
        {
            if (!database.IsRegistrationExpired(registration)) continue;
            
            logger.LogInfo(RefreshContext.Worker, $"Removed {registration.Username}'s queued registration since it has expired");
            database.RemoveRegistrationFromQueue(registration);
        }
        
        foreach (EmailVerificationCode code in database.GetAllVerificationCodes().Items)
        {
            if (!database.IsVerificationCodeExpired(code)) continue;
            
            logger.LogInfo(RefreshContext.Worker, $"Removed {code.User}'s verification code since it has expired");
            database.RemoveEmailVerificationCode(code);
        }
        
        foreach (Token token in database.GetAllTokens().Items)
        {
            if (!database.IsTokenExpired(token)) continue;
            
            logger.LogInfo(RefreshContext.Worker, $"Removed {token.User}'s {token.TokenType} token since it has expired {DateTimeOffset.Now - token.ExpiresAt} ago");
            database.RevokeToken(token);
        }
    }
}