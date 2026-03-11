// Uncomment to enable extra logging.
// Do not enable in production as this will flood your console and might be memory-heavy.
//#define CACHE_DEBUG

#if !DEBUG
#undef CACHE_DEBUG
#endif

using System.Diagnostics;
using Bunkum.Core.Services;
using MongoDB.Bson;
using NotEnoughLogs;
using Refresh.Common;
using Refresh.Core.Types.Cache;
using Refresh.Core.Types.Relations;
using Refresh.Database;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace Refresh.Core.Services;

public class CacheService : EndpointService
{
    private readonly TimeProviderService _time;
    private readonly Dictionary<string, CachedData<GameAsset?>> _cachedAssetData = []; // hash -> data
    private readonly Dictionary<int, CachedData<List<GameSkillReward>>> _cachedSkillRewards = []; // level ID -> data
    private readonly Dictionary<int, CachedData<List<Tag>>> _cachedLevelTags = []; // level ID -> data

    private readonly Dictionary<ObjectId, Dictionary<ObjectId, CachedData<OwnUserRelations>>> _cachedOwnUserRelations = []; // source user UUID -> target user UUID -> data
    private readonly Dictionary<ObjectId, Dictionary<int, CachedData<OwnLevelRelations>>> _cachedOwnLevelRelations = []; // source user UUID -> level ID -> data

    private const int LevelCacheDurationSeconds = 60 * 10;
    private const int AssetCacheDurationSeconds = 60 * 60; // GameAssets basically never change for now

    // TODO: unit tests for this
    public CacheService(Logger logger, TimeProviderService time) : base(logger)
    {
        this._time = time;
    }

    [Conditional("CACHE_DEBUG")]
    private void Log(ReadOnlySpan<char> format, params object[] args)
    {
        this.Logger.LogDebug(RefreshContext.CacheService, format, args);
    }

    public override void Initialize()
    {
        base.Initialize();

        // periodically automatically remove expired data if it hasn't been removed somehow else before,
        // to not keep unused data for a potentially undefined time
        Thread expirationThread = new(() =>
        {
            while(true)
            {
                Thread.Sleep(1000 * 60 * 2);
                this.RemoveExpiredCache();
            }
        });
        expirationThread.Start();
    }

    private void RemoveExpiredCache()
    {
        this.Log("Automatically removing expired cache...");
        DateTimeOffset now = this._time.TimeProvider.Now;

        foreach(KeyValuePair<string, CachedData<GameAsset?>> cache in this._cachedAssetData)
        {
            if (this.HasCacheExpired(cache.Value, now))
            {
                this.Log("Automatically removing expired GameAsset taken by hash {0}", cache.Key);
                this._cachedAssetData.Remove(cache.Key);
            }
        }

        foreach(KeyValuePair<int, CachedData<List<GameSkillReward>>> cache in this._cachedSkillRewards)
        {
            if (this.HasCacheExpired(cache.Value, now))
            {
                this.Log("Automatically removing expired GameSkillReward list taken from level {0}", cache.Key);
                this._cachedSkillRewards.Remove(cache.Key);
            }
        }

        foreach(KeyValuePair<int, CachedData<List<Tag>>> cache in this._cachedLevelTags)
        {
            if (this.HasCacheExpired(cache.Value, now))
            {
                this.Log("Automatically removing expired Tag list taken from level {0}", cache.Key);
                this._cachedLevelTags.Remove(cache.Key);
            }
        }

        foreach(KeyValuePair<ObjectId, Dictionary<ObjectId, CachedData<OwnUserRelations>>> cache in this._cachedOwnUserRelations)
        {
            foreach(KeyValuePair<ObjectId, CachedData<OwnUserRelations>> innerCache in cache.Value)
            {
                if (this.HasCacheExpired(innerCache.Value, now)) 
                {
                    this.Log("Automatically removing expired OwnUserRelations by user {0} for user {1}", cache.Key, innerCache.Key);
                    cache.Value.Remove(innerCache.Key);
                }
            }

            if (cache.Value.Count <= 0)
            {
                this.Log("Automatically removing empty OwnUserRelations dictionary by user {0}", cache.Key);
                this._cachedOwnUserRelations.Remove(cache.Key);
            }
        }

        foreach(KeyValuePair<ObjectId, Dictionary<int, CachedData<OwnLevelRelations>>> cache in this._cachedOwnLevelRelations)
        {
            foreach(KeyValuePair<int, CachedData<OwnLevelRelations>> innerCache in cache.Value)
            {
                if (this.HasCacheExpired(innerCache.Value, now)) 
                {
                    this.Log("Automatically removing expired OwnLevelRelations by user {0} for level {1}", cache.Key, innerCache.Key);
                    cache.Value.Remove(innerCache.Key);
                }
            }

            if (cache.Value.Count <= 0)
            {
                this.Log("Automatically removing empty OwnLevelRelations dictionary by user {0}", cache.Key);
                this._cachedOwnLevelRelations.Remove(cache.Key);
            }
        }
    }

    private bool HasCacheExpired<TData>(CachedData<TData>? cached, DateTimeOffset? now = null)
        => cached == null || cached.ExpiresAt < (now ?? this._time.TimeProvider.Now);

    #region Assets
    public void CacheAsset(string hash, GameAsset? asset)
    {
        if (hash.StartsWith('g') || hash.IsBlankHash()) return;

        DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(AssetCacheDurationSeconds);
        this.Log("Refreshing GameAsset {0} cache, it will expire at {1}", hash, expiresAt);
        this._cachedAssetData[hash] = new(asset, expiresAt);
    }

    // currently not necessary to return these with CachedReturn...
    public GameAsset? GetAssetInfo(string hash, GameDatabaseContext database)
    {
        if (hash.StartsWith('g') || hash.IsBlankHash()) return null;

        this.Log("Looking up GameAsset {0} in cache", hash);
        CachedData<GameAsset?>? fromCache = this._cachedAssetData.GetValueOrDefault(hash);

        if (this.HasCacheExpired(fromCache))
        {
            this.Log("Looking up GameAsset {0} in DB", hash);
            GameAsset? refreshed = database.GetAssetFromHash(hash);

            // Cache anyway, even if asset was not found in DB, for the same reason we're caching everything else
            // If the asset ever happens to be added to DB, it will be added to cache aswell by whatever is handling the upload/import anyway
            this.CacheAsset(hash, refreshed);
            return refreshed;
        }

        this.Log("Found unexpired GameAsset {0} in cache, it will expire at {1}", hash, fromCache!.ExpiresAt);
        return fromCache!.Content;
    }

    #endregion

    public void RemoveLevelData(GameLevel level)
    {
        this.RemoveSkillRewards(level);
        this.RemoveTags(level);
    }

    #region Skill Rewards

    public void CacheSkillRewards(GameLevel level, List<GameSkillReward> rewards)
    {
        DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(LevelCacheDurationSeconds);
        this.Log("Refreshing GameSkillRewards cache for level {0}, they will expire at {1}", level, expiresAt);
        this._cachedSkillRewards[level.LevelId] = new(rewards, expiresAt);
    }

    public void RemoveSkillRewards(GameLevel level)
    {
        this.Log("Removing GameSkillRewards cache for level {0}", level);
        this._cachedSkillRewards.Remove(level.LevelId);
    }

    public List<GameSkillReward> GetSkillRewards(GameLevel level, GameDatabaseContext database)
    {
        this.Log("Looking up GameSkillRewards for level {0} in cache", level);
        CachedData<List<GameSkillReward>>? fromCache = this._cachedSkillRewards.GetValueOrDefault(level.LevelId);

        if (this.HasCacheExpired(fromCache))
        {
            this.Log("Looking up GameSkillRewards for level {0} in DB", level);
            List<GameSkillReward> refreshed = database.GetSkillRewardsForLevel(level).ToList();

            this.CacheSkillRewards(level, refreshed);
            return refreshed;
        }

        this.Log("Found unexpired GameSkillRewards for level {0} in cache, they will expire at {1}", level, fromCache!.ExpiresAt);
        return fromCache!.Content;
    }

    #endregion
    #region Tags

    public void CacheTags(GameLevel level, List<Tag> tags)
    {
        DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(LevelCacheDurationSeconds);
        this.Log("Refreshing Tags cache for level {0}, they will expire at {1}", level, expiresAt);
        this._cachedLevelTags[level.LevelId] = new(tags, expiresAt);
    }

    // Make service re-get tags the next time they're requested, incase a tag is added or removed.
    // Can't just lazily add or remove tag to/from the cache because they're grouped and sorted by frequency (data we don't have here).
    public void RemoveTags(GameLevel level)
    {
        this.Log("Removing Tags cache for level {0}", level);
        this._cachedLevelTags.Remove(level.LevelId);
    }

    public List<Tag> GetTags(GameLevel level, GameDatabaseContext database)
    {
        this.Log("Looking up Tags for level {0} in cache", level);
        CachedData<List<Tag>>? fromCache = this._cachedLevelTags.GetValueOrDefault(level.LevelId);

        if (this.HasCacheExpired(fromCache))
        {
            this.Log("Looking up Tags for level {0} in DB", level);
            List<Tag> refreshed = database.GetTagsForLevel(level).ToList();

            this.CacheTags(level, refreshed);
            return refreshed;
        }

        this.Log("Found unexpired GameSkillRewards for level {0} in cache, they will expire at {1}", level, fromCache!.ExpiresAt);
        return fromCache!.Content;
    }

    #endregion

    #region Own User Relations

    public void CacheOwnUserRelations(GameUser source, GameUser target, OwnUserRelations newData)
    {
        DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(LevelCacheDurationSeconds);
        if (!this._cachedOwnUserRelations.ContainsKey(source.UserId)) 
            this._cachedOwnUserRelations[source.UserId] = [];

        this.Log("Refreshing OwnUserRelations cache by user {0} for user {1}, they will expire at {2}", source, target, expiresAt);
        this._cachedOwnUserRelations[source.UserId][target.UserId] = new(newData, expiresAt);
    }

    public void RemoveOwnUserRelations(GameUser source, GameUser target)
    {
        this.Log("Resetting OwnUserRelations by user {0}", source);
        this._cachedOwnUserRelations.GetValueOrDefault(source.UserId)?.Remove(target.UserId);
    }

    public CachedReturn<OwnUserRelations> GetOwnUserRelations(GameUser source, GameUser target, GameDatabaseContext database)
    {
        this.Log("Looking up OwnUserRelations by user {0} for user {0} in cache", source, target);
        CachedData<OwnUserRelations>? fromCache = this._cachedOwnUserRelations.GetValueOrDefault(source.UserId)?.GetValueOrDefault(target.UserId);

        if (this.HasCacheExpired(fromCache))
        {
            this.Log("Looking up OwnUserRelations by user {0} for user {1} in DB", source, target);
            OwnUserRelations refreshed = new()
            {
                IsHearted = database.IsUserFavouritedByUser(target, source),
            };

            this.CacheOwnUserRelations(source, target, refreshed);
            return new(refreshed, true);
        }

        this.Log("Found unexpired OwnUserRelations by user {0} for user {1} in cache, they will expire at {2}", source, target, fromCache!.ExpiresAt);
        return new(fromCache!.Content, false);
    }

    public void UpdateUserHeartedStatusByUser(GameUser source, GameUser target, bool newValue, GameDatabaseContext database)
    {
        CachedReturn<OwnUserRelations> fromCache = this.GetOwnUserRelations(source, target, database);
        if (fromCache.WasRefreshed) return; // value is already up-to-date

        // dictionaries are already ensured to exist
        this.Log("Lazily setting hearted status by user {0} for user {1} to {2}", source, target, newValue);
        this._cachedOwnUserRelations[source.UserId][target.UserId].Content.IsHearted = newValue;
    }

    #endregion
    #region Own Level Relations

    public void CacheOwnLevelRelations(GameUser source, GameLevel target, OwnLevelRelations newData)
    {
        DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(LevelCacheDurationSeconds);
        this.Log("Refreshing OwnUserRelations cache by user {0} for level {1}, they will expire at {2}", source, target, expiresAt);

        if (!this._cachedOwnLevelRelations.ContainsKey(source.UserId)) 
            this._cachedOwnLevelRelations[source.UserId] = [];

        this._cachedOwnLevelRelations[source.UserId][target.LevelId] = new(newData, expiresAt);
    }

    public CachedReturn<OwnLevelRelations> GetOwnLevelRelations(GameUser source, GameLevel target, GameDatabaseContext database)
    {
        this.Log("Looking up OwnLevelRelations by user {0} for level {0} in cache", source, target);
        CachedData<OwnLevelRelations>? fromCache = this._cachedOwnLevelRelations.GetValueOrDefault(source.UserId)?.GetValueOrDefault(target.LevelId);

        if (this.HasCacheExpired(fromCache))
        {
            this.Log("Looking up OwnUserRelations by user {0} for level {1} in DB", source, target);
            OwnLevelRelations refreshed = new()
            {
                IsHearted = database.IsLevelFavouritedByUser(target, source),
                IsQueued = database.IsLevelQueuedByUser(target, source),
                LevelRating = (int?)database.GetRatingByUser(target, source) ?? 0,
                TotalPlayCount = database.GetTotalPlaysForLevelByUser(target, source),
                TotalCompletionCount = database.GetTotalCompletionsForLevelByUser(target, source),
                PhotoCount = database.GetTotalPhotosInLevelByUser(target, source),
            };

            this.CacheOwnLevelRelations(source, target, refreshed);
            return new(refreshed, true);
        }

        this.Log("Found unexpired OwnLevelRelations by user {0} for level {1} in cache, they will expire at {2}", source, target, fromCache!.ExpiresAt);
        return new(fromCache!.Content, false);
    }

    public void UpdateLevelHeartedStatusByUser(GameUser source, GameLevel target, bool newValue, GameDatabaseContext database)
    {
        CachedReturn<OwnLevelRelations> fromCache = this.GetOwnLevelRelations(source, target, database);
        if (fromCache.WasRefreshed) return; // value is already up-to-date

        // dictionaries are already ensured to exist
        this.Log("Lazily setting hearted status by user {0} for level {1} to {2}", source, target, newValue);
        this._cachedOwnLevelRelations[source.UserId][target.LevelId].Content.IsHearted = newValue;
    }

    public void UpdateLevelQueuedStatusByUser(GameUser source, GameLevel target, bool newValue, GameDatabaseContext database)
    {
        CachedReturn<OwnLevelRelations> fromCache = this.GetOwnLevelRelations(source, target, database);
        if (fromCache.WasRefreshed) return; // value is already up-to-date

        // dictionaries are already ensured to exist
        this.Log("Lazily setting queued status by user {0} for level {1} to {2}", source, target, newValue);
        this._cachedOwnLevelRelations[source.UserId][target.LevelId].Content.IsQueued = newValue;
    }

    public void ClearQueueByUser(GameUser source)
    {
        this.Log("Resetting queued stati by user {0}", source);

        foreach (KeyValuePair<int, CachedData<OwnLevelRelations>> relations in this._cachedOwnLevelRelations.GetValueOrDefault(source.UserId)?.ToArray() ?? [])
        {
            // will be refreshed later anyway; don't want too many DB calls here (e.g. user had 100 levels queued, this method would in that case send 600 DB queries)
            if (this.HasCacheExpired(relations.Value)) continue;

            this.Log("Resetting queued status by user {0} for level {1}", source, relations.Key);
            relations.Value.Content.IsQueued = false;
            this._cachedOwnLevelRelations[source.UserId][relations.Key] = relations.Value;
        }
    }

    public void UpdateLevelRatingByUser(GameUser source, GameLevel target, int newRating, GameDatabaseContext database)
    {
        CachedReturn<OwnLevelRelations> fromCache = this.GetOwnLevelRelations(source, target, database);
        if (fromCache.WasRefreshed) return; // value is already up-to-date

        // dictionaries are already ensured to exist
        this.Log("Lazily setting level rating by user {0} for level {1} to {2}", source, target, newRating);
        this._cachedOwnLevelRelations[source.UserId][target.LevelId].Content.LevelRating = newRating;
    }

    public void IncrementLevelTotalPlaysByUser(GameUser source, GameLevel target, int incrementor, GameDatabaseContext database)
    {
        CachedReturn<OwnLevelRelations> fromCache = this.GetOwnLevelRelations(source, target, database);
        if (fromCache.WasRefreshed) return; // value is already up-to-date

        // dictionaries are already ensured to exist
        this.Log("Lazily incrementing total plays by user {0} for level {1} to {2}", source, target, incrementor);
        this._cachedOwnLevelRelations[source.UserId][target.LevelId].Content.TotalPlayCount += incrementor;
    }

    public void IncrementLevelTotalCompletionsByUser(GameUser source, GameLevel target, int incrementor, GameDatabaseContext database)
    {
        CachedReturn<OwnLevelRelations> fromCache = this.GetOwnLevelRelations(source, target, database);
        if (fromCache.WasRefreshed) return; // value is already up-to-date

        // dictionaries are already ensured to exist
        this.Log("Lazily incrementing total completions by user {0} for level {1} to {2}", source, target, incrementor);
        this._cachedOwnLevelRelations[source.UserId][target.LevelId].Content.TotalCompletionCount += incrementor;
    }

    public void ResetLevelCompletionCountByUser(GameUser source)
    {
        this.Log("Resetting total completions by user {0}", source);

        foreach (KeyValuePair<int, CachedData<OwnLevelRelations>> relations in this._cachedOwnLevelRelations.GetValueOrDefault(source.UserId)?.ToArray() ?? [])
        {
            if (this.HasCacheExpired(relations.Value)) continue;

            this.Log("Resetting total completions by user {0} for level {1}", source, relations.Key);
            relations.Value.Content.TotalCompletionCount = 0;
            this._cachedOwnLevelRelations[source.UserId][relations.Key] = relations.Value;
        }
    }

    public void IncrementLevelPhotosByUser(GameUser source, GameLevel target, int incrementor, GameDatabaseContext database)
    {
        CachedReturn<OwnLevelRelations> fromCache = this.GetOwnLevelRelations(source, target, database);
        if (fromCache.WasRefreshed) return; // value is already up-to-date

        // dictionaries are already ensured to exist
        this.Log("Lazily incrementing total photos by user {0} for level {1} to {2}", source, target, incrementor);
        this._cachedOwnLevelRelations[source.UserId][target.LevelId].Content.PhotoCount += incrementor;
    }

    public void ResetLevelPhotoCountsByUser(GameUser source)
    {
        this.Log("Resetting total photos by user {0}", source);

        foreach (KeyValuePair<int, CachedData<OwnLevelRelations>> relations in this._cachedOwnLevelRelations.GetValueOrDefault(source.UserId)?.ToArray() ?? [])
        {
            if (this.HasCacheExpired(relations.Value)) continue;

            this.Log("Resetting total photos by user {0} for level {1}", source, relations.Key);
            relations.Value.Content.PhotoCount = 0;
            this._cachedOwnLevelRelations[source.UserId][relations.Key] = relations.Value;
        }
    }

    #endregion
}