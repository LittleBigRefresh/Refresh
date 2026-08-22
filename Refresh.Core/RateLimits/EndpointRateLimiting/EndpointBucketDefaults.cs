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
        
        #region Levels
        // game sometimes requests many levels in bursts
        {EndpointBucketId.GameGetSingleLevel, new(240, 200, 180)},
        {EndpointBucketId.ApiGetSingleLevel, new(240, 50, 180)},
        #endregion
    }.ToFrozenDictionary();
}