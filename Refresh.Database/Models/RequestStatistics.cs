namespace Refresh.Database.Models;

public partial class RequestStatistics : IRealmObject
{
    [Key, Ignored] public int Id { get; set; }
    
    public long TotalRequests { get; set; }
    public long ApiRequests { get; set; }
    public long GameRequests { get; set; }
}