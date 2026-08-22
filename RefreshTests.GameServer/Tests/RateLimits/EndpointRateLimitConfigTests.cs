using Refresh.Core.Configuration;
using Refresh.Core.RateLimits.EndpointRateLimiting;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using RefreshTests.GameServer.GameServer.Configuration;

namespace RefreshTests.GameServer.Tests.RateLimits;

public class EndpointRateLimitConfigTests : GameServerTest
{
    [Test]
    public void EnsureDefaultMapHasAllBucketIdsFromEnum()
    {
        foreach (EndpointBucketId bucket in Enum.GetValues<EndpointBucketId>())
        {
            Assert.That(EndpointBucketDefaults.Buckets.ContainsKey(bucket), Is.True);
        }
    }
    
    [Test]
    public void ConfigCopiesAllDefaultsOnInit()
    {
        TestEndpointRateLimitConfig config = new();
        config.TestMigration(); // ensure that migration from version 0 -> 1 triggers population automatically
        
        Assert.That(config.Buckets.Count, Is.GreaterThan(0));
        foreach (KeyValuePair<EndpointBucketId, ConfigRateLimitBucket> defaultPair in EndpointBucketDefaults.Buckets)
        {
            string bucketIdSerialized = defaultPair.Key.ToString();
            ConfigRateLimitBucket? configPair = config.Buckets.GetValueOrDefault(bucketIdSerialized);
            
            Assert.That(configPair, Is.Not.Null);
            Assert.That(configPair!.TimeWindowSeconds, Is.EqualTo(defaultPair.Value.TimeWindowSeconds));
            Assert.That(configPair!.MaxRequestAmount, Is.EqualTo(defaultPair.Value.MaxRequestAmount));
            Assert.That(configPair!.BlockDurationSeconds, Is.EqualTo(defaultPair.Value.BlockDurationSeconds));
        }
    }
    
    [Test]
    public void TestConfiguredBucketsOnVariousEndpoints()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevelWithRootResource(user, "12345");
        
        context.Server.Value.Server.AddEndpointGroup<TestEndpoints>();
        using HttpClient authedGameClient = context.GetAuthenticatedClient(TokenType.Game, user);
        
        EndpointRateLimitConfig config = context.Server.Value.EndpointRateLimitConfig;
        config.AddMissingBucketsFromDefaults();
        
        // ensure buckets are in the config
        Assert.That(config.Buckets.ContainsKey(nameof(EndpointBucketId.ApiGetSingleLevel)), Is.True);
        Assert.That(config.Buckets.ContainsKey(nameof(EndpointBucketId.GameGetSingleLevel)), Is.True);
        Assert.That(config.Buckets.ContainsKey(nameof(EndpointBucketId.Default)), Is.True);
        
        // set bucket max requests to testable values
        config.Buckets[nameof(EndpointBucketId.ApiGetSingleLevel)].MaxRequestAmount = 1;
        config.Buckets[nameof(EndpointBucketId.GameGetSingleLevel)].MaxRequestAmount = 2;
        config.Buckets[nameof(EndpointBucketId.Default)].MaxRequestAmount = 3;
        
        // ensure that the endpoints use their configured rate-limit.
        // this also ensures that rate-limits are enforced for both authed and unauthed users.
        this.TriggerRateLimit(context.Http, $"/api/v3/levels/id/{level.LevelId}", 1);
        this.TriggerRateLimit(authedGameClient, $"/lbp/s/user/{level.LevelId}", 2);
        
        // this one should fall back to the default bucket.
        // these test endpoints will definitely not have any dedicated buckets in the future
        this.TriggerRateLimit(context.Http, $"/api/v3/test", 3);
        
        // ensure this one is already blocked because we've spammed the other endpoint from the same bucket
        this.TriggerRateLimit(context.Http, $"/api/v3/levels/hash/{level.RootResource}", 0);
    }

    private void TriggerRateLimit(HttpClient client, string endpoint, int maxRequestAmount)
    {
        for (int i = 0; i < maxRequestAmount; i++)
        {
            HttpResponseMessage message = client.GetAsync(endpoint).Result;
            Assert.That(message.IsSuccessStatusCode, Is.True);
        }
        
        // Now ensure we're being rate-limited because we reached the limit
        HttpResponseMessage rateLimitMessage = client.GetAsync(endpoint).Result;
        Assert.That(rateLimitMessage.IsSuccessStatusCode, Is.False);
        Assert.That(rateLimitMessage.StatusCode, Is.EqualTo(TooManyRequests));
    }
}