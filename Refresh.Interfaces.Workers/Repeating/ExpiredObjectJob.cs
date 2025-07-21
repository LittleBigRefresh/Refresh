using Refresh.Core;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers.Repeating;

public class CleanupExpiredObjectsJob : RepeatingJob
{
    protected override int Interval => 60_000; // 1 minute
    public override void ExecuteJob(WorkContext context)
    {
        List<QueuedRegistration> registrationsToRemove = [];
        List<EmailVerificationCode> codesToRemove = [];
        List<Token> tokensToRemove = [];
        
        // gather
        foreach (QueuedRegistration registration in context.Database.GetAllQueuedRegistrations().Items)
        {
            if (!context.Database.IsRegistrationExpired(registration)) continue;
            
            context.Logger.LogInfo(RefreshContext.Worker, $"Removed {registration.Username}'s queued registration since it has expired");
            registrationsToRemove.Add(registration);
        }
        
        foreach (EmailVerificationCode code in context.Database.GetAllVerificationCodes().Items)
        {
            if (!context.Database.IsVerificationCodeExpired(code)) continue;
            
            context.Logger.LogInfo(RefreshContext.Worker, $"Removed {code.User}'s verification code since it has expired");
            codesToRemove.Add(code);
        }
        
        foreach (Token token in context.Database.GetAllTokens().Items)
        {
            if (!context.Database.IsTokenExpired(token)) continue;
            
            context.Logger.LogInfo(RefreshContext.Worker, $"Removed {token.User}'s {token.TokenType} token since it has expired {DateTimeOffset.Now - token.ExpiresAt} ago");
            tokensToRemove.Add(token);
        }
        
        // remove
        foreach (QueuedRegistration registration in registrationsToRemove)
        {
            context.Database.RemoveRegistrationFromQueue(registration);
        }
        
        foreach (EmailVerificationCode code in codesToRemove)
        {
            context.Database.RemoveEmailVerificationCode(code);
        }
        
        foreach (Token token in tokensToRemove)
        {
            context.Database.RevokeToken(token);
        }
    }
}