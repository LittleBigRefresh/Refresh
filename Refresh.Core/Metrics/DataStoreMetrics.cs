using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Refresh.Core.Metrics;

/// <summary>
/// Metrics regarding timings and totals of DataStores.
/// </summary>
public static class DataStoreMetrics
{
    private static readonly Meter Meter = new("refresh.core.storage");
    
    private static readonly Counter<int> AssetsServed = Meter.CreateCounter<int>("refresh.core.storage.served");
    private static readonly Histogram<double> AssetsTime = Meter.CreateHistogram<double>("refresh.core.storage.timing");

    private static readonly Counter<int> RemoteDataStoreChecks = Meter.CreateCounter<int>("refresh.core.storage.remote.checks");

    private static void Record(Stopwatch sw, string type)
    {
        KeyValuePair<string, object?> kvp = new("dataStore", type);
        AssetsServed.Add(1, kvp);
        AssetsTime.Record(sw.Elapsed.TotalMilliseconds, kvp);
        sw.Stop();
    }

    public static void RecordDry(Stopwatch sw) => Record(sw, "dry");
    public static void RecordRemote(Stopwatch sw) => Record(sw, "remote");
    public static void RecordRemoteCheck() => RemoteDataStoreChecks.Add(1);
}