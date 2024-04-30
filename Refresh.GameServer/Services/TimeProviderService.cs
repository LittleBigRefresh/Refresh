using System.Reflection;
using Bunkum.Core;
using Bunkum.Listener.Request;
using Bunkum.Core.Database;
using Bunkum.Core.Services;
using NotEnoughLogs;
using Refresh.GameServer.Time;

namespace Refresh.GameServer.Services;

public class TimeProviderService : Service
{
    public IDateTimeProvider TimeProvider { get; }

    internal TimeProviderService(Logger logger, IDateTimeProvider timeProvider) : base(logger)
    {
        this.TimeProvider = timeProvider;
    }

    public override object? AddParameterToEndpoint(ListenerContext context, BunkumParameterInfo paramInfo, Lazy<IDatabaseContext> database)
    {
        if (paramInfo.ParameterType == typeof(IDateTimeProvider))
        {
            return this.TimeProvider;
        }

        return null;
    }
}