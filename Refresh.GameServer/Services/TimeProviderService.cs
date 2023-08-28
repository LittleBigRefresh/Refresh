using System.Reflection;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Services;
using NotEnoughLogs;
using Refresh.GameServer.Time;

namespace Refresh.GameServer.Services;

public class TimeProviderService : Service
{
    private readonly IDateTimeProvider _timeProvider;

    internal TimeProviderService(LoggerContainer<BunkumContext> logger, IDateTimeProvider timeProvider) : base(logger)
    {
        this._timeProvider = timeProvider;

    }

    public override object? AddParameterToEndpoint(ListenerContext context, ParameterInfo paramInfo, Lazy<IDatabaseContext> database)
    {
        if (paramInfo.ParameterType == typeof(IDateTimeProvider))
        {
            return this._timeProvider;
        }

        return null;
    }
}