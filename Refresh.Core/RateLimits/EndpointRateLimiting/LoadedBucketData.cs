using Refresh.Core.Configuration;

namespace Refresh.Core.RateLimits.EndpointRateLimiting;

public record LoadedBucketData(EndpointBucketId Id, ConfigRateLimitBucket Data);