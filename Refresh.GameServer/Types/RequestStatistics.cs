using Microsoft.EntityFrameworkCore;
using Realms;

namespace Refresh.GameServer.Types;

[Keyless] // TODO: AGONY
public partial class RequestStatistics : IRealmObject
{
    public long TotalRequests { get; set; }
    public long ApiRequests { get; set; }
    public long LegacyApiRequests { get; set; }
    public long GameRequests { get; set; }
}