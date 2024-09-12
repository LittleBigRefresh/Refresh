using Bunkum.Core.Storage;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.OAuth2.Discord;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Workers;

public class ExpiredObjectWorker : IWorker
{
    public int WorkInterval => 60_000; // 1 minute
    public void DoWork(DataContext context)
    {
        foreach (QueuedRegistration registration in context.Database.GetAllQueuedRegistrations().Items)
        {
            if (!context.Database.IsRegistrationExpired(registration)) continue;
            
            context.Logger.LogInfo(RefreshContext.Worker, $"Removed {registration.Username}'s queued registration since it has expired");
            context.Database.RemoveRegistrationFromQueue(registration);
        }
        
        foreach (EmailVerificationCode code in context.Database.GetAllVerificationCodes().Items)
        {
            if (!context.Database.IsVerificationCodeExpired(code)) continue;
            
            context.Logger.LogInfo(RefreshContext.Worker, $"Removed {code.User}'s verification code since it has expired");
            context.Database.RemoveEmailVerificationCode(code);
        }
        
        foreach (Token token in context.Database.GetAllTokens().Items)
        {
            if (!context.Database.IsTokenExpired(token)) continue;
            
            context.Logger.LogInfo(RefreshContext.Worker, $"Removed {token.User}'s {token.TokenType} token since it has expired {DateTimeOffset.Now - token.ExpiresAt} ago");
            context.Database.RevokeToken(token);
        }
        
        context.Database.RemoveAllExpiredDiscordOAuth2Requests(context.TimeProvider);
    }
}