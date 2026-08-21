using Bunkum.Core.Configuration;
using Refresh.Core.RateLimits.EndpointRateLimiting.Buckets;

namespace Refresh.Core.Configuration;

public class EndpointRateLimitConfig : Config
{
    public override int CurrentConfigVersion => 1;
    public override int Version { get; set; }
    
    protected override void Migrate(int oldVer, dynamic oldConfig)
    {
        // initialize
        if (oldVer < 1)
        {
            this.AddMissingBucketsFromDefaults();
        }
        
        // think of how exactly to overwrite in the future
    }
    
    private void AddMissingBucketsFromDefaults()
    {
        // then fill in missing buckets from defaults
        foreach (KeyValuePair<EndpointBucketName, ConfigRateLimitBucket> defaultPair in EndpointBucketDefaults.Buckets)
        {
            string bucketName = defaultPair.Key.ToString();
            ConfigRateLimitBucket bucket = defaultPair.Value;
            this.Buckets.TryAdd(bucketName, bucket);
        }
    }

    /// <summary>
    /// If a bucket's default values are updated in a new server release, this will determine whether the bucket's configured values,
    /// which might or might not have been changed by the server owner, will be overwritten with the new default or not.
    /// 
    /// Although this option does nothing for now, it already exists so owners can already decide to opt out of this ahead of time.
    /// </summary>
    public bool OverwriteBucketValuesIfDefaultsAreUpdated { get; set; } = true;
    public Dictionary<string, ConfigRateLimitBucket> Buckets { get; set; } = new();
}