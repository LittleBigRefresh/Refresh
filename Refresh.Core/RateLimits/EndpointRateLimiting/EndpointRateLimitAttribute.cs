namespace Refresh.Core.RateLimits.EndpointRateLimiting;

[AttributeUsage(AttributeTargets.Method)]
public class EndpointRateLimitAttribute : Attribute
{
    public readonly EndpointBucketId MainBucket;

    public EndpointRateLimitAttribute(EndpointBucketId bucket)
    {
        this.MainBucket = bucket;
    }
}