namespace Refresh.Core.RateLimits.EndpointRateLimiting;

[AttributeUsage(AttributeTargets.Method)]
public class EndpointRateLimitAttribute : Attribute
{
    public readonly EndpointBucketId MainBucket;

    /// <summary>
    /// If the client is LBP PSP, use this bucket instead of MainBucket.
    /// We need this secondary bucket because LBP PSP uses the same endpoints as LBP1,
    /// while also sending higher amounts of requests to certain endpoints in certain cases.
    /// </summary>
    public readonly EndpointBucketId PspBucket;

    public EndpointRateLimitAttribute(EndpointBucketId bucket, EndpointBucketId pspBucket)
    {
        this.MainBucket = bucket;
        this.PspBucket = pspBucket;
    }

    public EndpointRateLimitAttribute(EndpointBucketId bucket)
    {
        this.MainBucket = bucket;
        this.PspBucket = bucket;
    }
}