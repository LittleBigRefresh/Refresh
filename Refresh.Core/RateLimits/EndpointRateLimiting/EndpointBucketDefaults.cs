using System.Collections.Frozen;
using Refresh.Core.Configuration;

namespace Refresh.Core.RateLimits.EndpointRateLimiting;

public static class EndpointBucketDefaults
{
    public static readonly FrozenDictionary<EndpointBucketId, ConfigRateLimitBucket> Buckets = new Dictionary<EndpointBucketId, ConfigRateLimitBucket>()
    {
        #region Misc
        {EndpointBucketId.Default, new(90, 300, 45)},
        #endregion
    }.ToFrozenDictionary();
}