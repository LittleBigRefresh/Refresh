using System.Collections.Frozen;

namespace Refresh.Core.RateLimits.EndpointRateLimiting;

public static class EndpointBucketDefaults
{
    public static readonly FrozenDictionary<EndpointBucketId, string> Buckets = new Dictionary<EndpointBucketId, string>()
    {
        #region Misc
        {EndpointBucketId.Default, new(90, 300, 45)},
        #endregion
    }.ToFrozenDictionary();
}