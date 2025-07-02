using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Refresh.Core.Metrics;

/// <summary>
/// Metrics regarding timings and totals of certain events taking place during this instance of the game server.
/// </summary>
/// <remarks>
/// These are different from metrics like <seealso cref="RequestStatisticTrackingService"/> because those are stored totals in the database.
/// These are instead reported as dotnet metrics, which can be consumed by Prometheus and picked apart per-instance per-lifetime per-pod.
/// </remarks>
public static class GameServerMetrics
{
    private static readonly Meter Meter = new("refresh.gameserver");

    public static void RecordApiRequest(Stopwatch sw)
    {
        ApiRequestsServed.Add(1);
        ApiRequestTime.Record(sw.Elapsed.TotalMilliseconds);
        sw.Stop();
    }
    
    public static void RecordGameRequest(Stopwatch sw)
    {
        GameRequestsServed.Add(1);
        GameRequestTime.Record(sw.Elapsed.TotalMilliseconds);
        sw.Stop();
    }

    public static void RecordImageConversion(Stopwatch sw)
    {
        ImageConversions.Add(1);
        ImageConversionTime.Record(sw.Elapsed.TotalMilliseconds);
        sw.Stop();
    }
    
    private static readonly Counter<int> ApiRequestsServed = Meter.CreateCounter<int>("refresh.gameserver.requests_served.api");
    private static readonly Histogram<double> ApiRequestTime = Meter.CreateHistogram<double>("refresh.gameserver.request_timing.api");
    
    private static readonly Counter<int> GameRequestsServed = Meter.CreateCounter<int>("refresh.gameserver.requests_served.game");
    private static readonly Histogram<double> GameRequestTime = Meter.CreateHistogram<double>("refresh.gameserver.request_timing.game");

    private static readonly Counter<int> ImageConversions = Meter.CreateCounter<int>("refresh.gameserver.images_converted");
    private static readonly Histogram<double> ImageConversionTime = Meter.CreateHistogram<double>("refresh.gameserver.image_conversion_timing");
}