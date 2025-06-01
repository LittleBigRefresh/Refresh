using Bunkum.Core;
using Bunkum.Core.Database;
using Bunkum.Core.Services;
using Bunkum.Listener.Request;
using NotEnoughLogs;
using Refresh.Common.Time;

namespace Refresh.Core.Services;

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