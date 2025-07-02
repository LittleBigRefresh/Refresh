using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;

namespace Refresh.Core.Metrics;

/// <summary>
/// Metrics regarding timings and totals of certain events taking place during this instance of the game server.
/// </summary>
/// <remarks>
/// These are different from metrics like RequestStatisticTrackingService because those are stored totals in the database.
/// These are instead reported as dotnet metrics, which can be consumed by Prometheus and picked apart per-instance per-lifetime per-pod.
/// </remarks>
public static class GameServerMetrics
{
    private static readonly Meter Meter = new("refresh.gameserver");
    
    private static readonly Counter<int> ApiRequestsServed = Meter.CreateCounter<int>("refresh.gameserver.requests_served.api");
    private static readonly Histogram<double> ApiRequestTime = Meter.CreateHistogram<double>("refresh.gameserver.request_timing.api");
    private static readonly Counter<int> ApiRequestErrors = Meter.CreateCounter<int>("refresh.gameserver.request_errors.api");
    
    private static readonly Counter<int> GameRequestsServed = Meter.CreateCounter<int>("refresh.gameserver.requests_served.game");
    private static readonly Histogram<double> GameRequestTime = Meter.CreateHistogram<double>("refresh.gameserver.request_timing.game");
    private static readonly Counter<int> GameRequestErrors = Meter.CreateCounter<int>("refresh.gameserver.request_errors.api");

    private static readonly Counter<int> ImageConversions = Meter.CreateCounter<int>("refresh.gameserver.images_converted");
    private static readonly Histogram<double> ImageConversionTime = Meter.CreateHistogram<double>("refresh.gameserver.image_conversion_timing");

    private static bool IsError(HttpStatusCode code)
    {
        return (int)code >= 400;
    }

    public static void RecordApiRequest(Stopwatch sw, HttpStatusCode code)
    {
        ApiRequestsServed.Add(1);
        ApiRequestTime.Record(sw.Elapsed.TotalMilliseconds);
        
        if(IsError(code))
            ApiRequestErrors.Add(1, new KeyValuePair<string, object?>("code", code));
        
        sw.Stop();
    }
    
    public static void RecordGameRequest(Stopwatch sw, HttpStatusCode code)
    {
        GameRequestsServed.Add(1);
        GameRequestTime.Record(sw.Elapsed.TotalMilliseconds);
        
        if(IsError(code))
            GameRequestErrors.Add(1, new KeyValuePair<string, object?>("code", code));
        
        sw.Stop();
    }

    public static void RecordImageConversion(Stopwatch sw)
    {
        ImageConversions.Add(1);
        ImageConversionTime.Record(sw.Elapsed.TotalMilliseconds);
        sw.Stop();
    }
}