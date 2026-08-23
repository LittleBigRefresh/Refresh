using System.Collections.Frozen;
using System.Net;
using System.Reflection;
using Bunkum.Listener.Request;
using MongoDB.Bson;
using NotEnoughLogs;
using Refresh.Common;
using Refresh.Common.Time;
using Refresh.Core.Configuration;
using Refresh.Core.RateLimits.EndpointRateLimiting.Client;
using Refresh.Database.Models.Users;

namespace Refresh.Core.RateLimits.EndpointRateLimiting;

public class EndpointRateLimiter
{
    private readonly Logger _logger;
    private readonly IDateTimeProvider _timeProvider;
    private readonly FrozenDictionary<EndpointBucketId, ConfigRateLimitBucket> _buckets;

    private readonly List<TrackedClientBucketData<ObjectId>> _userInfos = new(25);
    private readonly List<TrackedClientBucketData<IPAddress>> _remoteEndpointInfos = new(25);

    public EndpointRateLimiter(IDateTimeProvider timeProvider, Logger logger, EndpointRateLimitConfig config)
    {
        this._timeProvider = timeProvider;
        this._logger = logger;

        // Copy the buckets over, converting the string bucket names to their corresponding enum values.
        Dictionary<EndpointBucketId, ConfigRateLimitBucket> validBuckets = new();

        foreach (KeyValuePair<string, ConfigRateLimitBucket> bucket in config.Buckets)
        {
            bool parsed = Enum.TryParse(bucket.Key, true, out EndpointBucketId nameParsed);
            if (!parsed)
            {
                this._logger.LogDebug(RefreshContext.RateLimit, $"Bucket name '{bucket.Key}' found in rate-limit config is unknown (does not map to a valid {nameof(EndpointBucketId)} enum value), its bucket will be ignored.");
                continue;
            }

            validBuckets.Add(nameParsed, bucket.Value);
        }
        
        // check for any buckets missing from the config, and insert default buckets in their place.
        // this way, instead of logging the missing bucket every single time it's looked up during a request,
        // we instead just print it once here.
        foreach (KeyValuePair<EndpointBucketId, ConfigRateLimitBucket> defaultPair in EndpointBucketDefaults.Buckets)
        {
            bool existsInConfig = validBuckets.ContainsKey(defaultPair.Key);
            if (existsInConfig) continue;

            if (config.PrintMissingBuckets)
            {
                logger.LogWarning(RefreshContext.RateLimit, $"Bucket {defaultPair.Key} is missing from your config, we will use its hardcoded defaults instead.");
            }
            
            validBuckets.Add(defaultPair.Key, defaultPair.Value);
        }

        this._buckets = validBuckets.ToFrozenDictionary();
    }

    private LoadedBucketData GetBucketNameAndData(ListenerContext context, MethodInfo? method)
    {
        EndpointRateLimitAttribute? attribute = method?.GetCustomAttribute<EndpointRateLimitAttribute>();

        EndpointBucketId bucketName = EndpointBucketId.Default;
        if (attribute != null) bucketName = attribute.MainBucket;

        ConfigRateLimitBucket? bucketData = this._buckets.GetValueOrDefault(bucketName);

        if (bucketData == null)
        {
            // Don't look this bucket up in the defaults, because we've already merged with the defaults in the constructor above,
            // so all buckets missing from the config should already have their default versions in the map we use here.
            throw new NotImplementedException($"Could not find bucket '{bucketName}' in neither the config file nor the hardcoded defaults! You should open an issue about this.");
        }

        return new LoadedBucketData(bucketName, bucketData);
    }

    public bool UserViolatesRateLimit(ListenerContext context, MethodInfo method, GameUser user)
    {
        LoadedBucketData bucketData = this.GetBucketNameAndData(context, method);

        lock (this._userInfos)
        {
            TrackedClientBucketData<ObjectId>? info = this._userInfos
                .FirstOrDefault(i => user.UserId.Equals(i.ClientId) && i.Bucket == bucketData.Id);

            if (info == null)
            {
                info = new TrackedClientBucketData<ObjectId>(user.UserId, bucketData.Id);
                this._userInfos.Add(info);
            }

            lock (info)
            {
                return this.ViolatesRateLimit(context, bucketData, info, user);
            }
        }
    }

    public bool RemoteEndpointViolatesRateLimit(ListenerContext context, MethodInfo method)
    {
        IPAddress ipAddress = context.RemoteEndpoint.Address;

        LoadedBucketData bucketData = this.GetBucketNameAndData(context, method);

        lock (this._remoteEndpointInfos)
        {
            TrackedClientBucketData<IPAddress>? info = this._remoteEndpointInfos
                .FirstOrDefault(i => ipAddress.Equals(i.ClientId) && i.Bucket == bucketData.Id);

            if (info == null)
            {
                info = new TrackedClientBucketData<IPAddress>(ipAddress, bucketData.Id);
                this._remoteEndpointInfos.Add(info);
            }

            lock (info)
            {
                return this.ViolatesRateLimit(context, bucketData, info, null);
            }
        }
    }

    public bool ViolatesRateLimit(ListenerContext context, LoadedBucketData bucket, IClientBucketBaseData info, GameUser? user)
    {
        int now = (int)this._timeProvider.TimestampSeconds;
        
        this._logger.LogTrace(RefreshContext.RateLimit, $"{this.GetType().Name}.{nameof(this.ViolatesRateLimit)}() - Request times count: {info.RequestTimes.Count}, limited until: {info.LimitedUntil}.");
        
        if (info.LimitedUntil != 0)
        {
            // TODO also track requests received while the client is already rate-limited, to increase their block duration as punishment
            if (info.LimitedUntil > now) return true;
            info.LimitedUntil = 0;
            
            // TODO don't clear all tracked requests once the block duration is over, only ever clear expired ones
            info.RequestTimes.Clear();
        }
        
        info.RequestTimes.RemoveAll(r => r <= now - bucket.Data.TimeWindowSeconds);
        
        if (info.RequestTimes.Count + 1 > bucket.Data.MaxRequestAmount)
        {
            info.LimitedUntil = now + bucket.Data.BlockDurationSeconds;
            context.ResponseHeaders.TryAdd("Retry-After", bucket.Data.BlockDurationSeconds.ToString());
            
            return true;
        }
        
        info.RequestTimes.Add(now);
        return false;
    }
}