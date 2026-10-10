using System.Reflection;
using Bunkum.Core;
using Bunkum.Core.Database;
using Bunkum.Core.Responses;
using Bunkum.Core.Services;
using Bunkum.Listener.Request;
using NotEnoughLogs;
using Refresh.Core.Attributes;

namespace Refresh.Core.Services;

public class OverrideStatusService : Service
{
    internal OverrideStatusService(Logger logger) : base(logger)
    {}
    
    public override void AfterRequestHandled(ListenerContext context, Response? response, MethodInfo method, Lazy<IDatabaseContext> database)
    {
        // If the client is LBP PSP, and it's an endpoint which may not return non-OK responses to PSP, override with OK,
        // else LBP PSP will explode or something.
        if (context.ResponseCode != OK && context.RequestHeaders.Get("User-Agent") == "LBPPSP CLIENT"
            && method.GetCustomAttributes<OverrideStatusIfPspAttribute>().Any())
        {
            this.Logger.LogInfo(BunkumCategory.Game, $"Overriding original response '{context.ResponseCode}' with OK because the client is LBP PSP.");
            context.ResponseCode = OK;
        }
    }
}