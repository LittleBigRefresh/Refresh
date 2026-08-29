namespace Refresh.Core.RateLimits.EndpointRateLimiting.Client;

public interface IClientBucketBaseData
{
    public List<int> RequestTimes { get; init; }
    public int LimitedUntil { get; set; }
}