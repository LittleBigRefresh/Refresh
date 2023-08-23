using Realms;

namespace Refresh.GameServer.Types;

public partial class RequestStatistics : IRealmObject
{
    public long TotalRequests { get; set; }
    public long ApiRequests { get; set; }
    public long LegacyApiRequests { get; set; }
    public long GameRequests { get; set; }
}