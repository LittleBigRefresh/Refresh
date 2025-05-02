namespace Refresh.Database.Models;

public partial class RequestStatistics : IRealmObject
{
    public long TotalRequests { get; set; }
    public long ApiRequests { get; set; }
    public long GameRequests { get; set; }
}