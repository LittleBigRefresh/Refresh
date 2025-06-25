namespace Refresh.Database.Models.Statistics;

public partial class RequestStatistics
{
    [Key] public int Id { get; set; }

    [NotMapped] public long TotalRequests => ApiRequests + GameRequests;
    public long ApiRequests { get; set; }
    public long GameRequests { get; set; }
}