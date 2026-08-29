using System.Collections.Frozen;
using System.Net;
using MongoDB.Bson;
using NotEnoughLogs;
using Refresh.Common.Time;
using Refresh.Core.Configuration;
using Refresh.Core.RateLimits.EndpointRateLimiting;
using Refresh.Core.RateLimits.EndpointRateLimiting.Client;

namespace RefreshTests.GameServer.GameServer;

public class TestEndpointRateLimiter : EndpointRateLimiter
{
    public TestEndpointRateLimiter(IDateTimeProvider timeProvider, Logger logger, EndpointRateLimitConfig config) : base(timeProvider, logger, config)
    {
        
    }

    public FrozenDictionary<EndpointBucketId,ConfigRateLimitBucket> GetBucketMap => this.Buckets;
    public List<TrackedClientBucketData<ObjectId>> GetUsers => this.UserInfos;
    public List<TrackedClientBucketData<IPAddress>> GetRemoteEndpoints => this.RemoteEndpointInfos;
}