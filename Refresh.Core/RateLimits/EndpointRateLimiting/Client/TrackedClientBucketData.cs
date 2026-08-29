namespace Refresh.Core.RateLimits.EndpointRateLimiting.Client;

public class TrackedClientBucketData<TClientIdType> : IClientBucketBaseData
{
    public List<int> RequestTimes { get; init; } = new(25);
    public int LimitedUntil { get; set; }
    public TClientIdType ClientId { get; init; }
    public EndpointBucketId Bucket { get; init; }

    public TrackedClientBucketData(TClientIdType clientId, EndpointBucketId bucket)
    {
        this.ClientId = clientId;
        this.Bucket = bucket;
    }
}