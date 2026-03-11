using Bunkum.Core.Services;
using MongoDB.Bson;
using NotEnoughLogs;
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

    // TODO: maybe save these in DB aswell?
    private readonly Dictionary<ObjectId, Dictionary<ObjectId, CachedData<OwnUserRelations>>> _cachedOwnUserRelations = []; // source user UUID -> target user UUID -> data
    private readonly Dictionary<ObjectId, Dictionary<int, CachedData<OwnLevelRelations>>> _cachedOwnLevelRelations = []; // source user UUID -> level ID -> data

    private const int LevelCacheDurationSeconds = 60 * 10;
    private const int AssetCacheDurationSeconds = 60 * 60; // GameAssets basically never change for now

    // TODO: some way to auto-remove cached stuff if expired
    public CacheService(Logger logger, TimeProviderService time) : base(logger)
    {
        this._time = time;
    }

    #region Assets
    public void CacheAsset(string hash, GameAsset? asset)
    {
        if (hash.StartsWith('g') || hash.IsBlankHash()) return;

        DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(AssetCacheDurationSeconds);
        this._cachedAssetData[hash] = new(asset, expiresAt);
    }

    // currently not necessary to return these with CachedReturn...
    public GameAsset? GetAssetInfo(string hash, GameDatabaseContext database)
    {
        if (hash.StartsWith('g') || hash.IsBlankHash()) return null;
        CachedData<GameAsset?>? fromCache = this._cachedAssetData.GetValueOrDefault(hash);

        if (this.HasCacheExpired(fromCache))
        {
            GameAsset? refreshed = database.GetAssetFromHash(hash);

            // Cache anyway, even if asset was not found in DB, for the same reason we're caching everything else
            // If the asset ever happens to be added to DB, it will be added to cache aswell by whatever is handling the upload/import anyway
            this.CacheAsset(hash, refreshed);
            return refreshed;
        }

        return fromCache?.Content;
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
        this._cachedSkillRewards[level.LevelId] = new(rewards, expiresAt);
    }

    public void RemoveSkillRewards(GameLevel level)
    {
        this._cachedSkillRewards.Remove(level.LevelId);
    }

    public List<GameSkillReward> GetSkillRewards(GameLevel level, GameDatabaseContext database)
    {
        CachedData<List<GameSkillReward>>? fromCache = this._cachedSkillRewards.GetValueOrDefault(level.LevelId);

        if (this.HasCacheExpired(fromCache))
        {
            List<GameSkillReward> refreshed = database.GetSkillRewardsForLevel(level).ToList();

            this.CacheSkillRewards(level, refreshed);
            return refreshed;
        }

        return fromCache!.Content;
    }

    #endregion
    #region Tags

    public void CacheTags(GameLevel level, List<Tag> tags)
    {
        DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(LevelCacheDurationSeconds);
        this._cachedLevelTags[level.LevelId] = new(tags, expiresAt);
    }

    // Make service re-get tags the next time they're requested, incase a tag is added or removed.
    // Can't just lazily add or remove tag to/from the cache because they're grouped and sorted by frequency (data we don't have here).
    public void RemoveTags(GameLevel level)
    {
        this._cachedLevelTags.Remove(level.LevelId);
    }

    public List<Tag> GetTags(GameLevel level, GameDatabaseContext database)
    {
        CachedData<List<Tag>>? fromCache = this._cachedLevelTags.GetValueOrDefault(level.LevelId);

        if (this.HasCacheExpired(fromCache))
        {
            List<Tag> refreshed = database.GetTagsForLevel(level).ToList();

            this.CacheTags(level, refreshed);
            return refreshed;
        }

        return fromCache!.Content;
    }

    #endregion

    #region Own User Relations

    public void CacheOwnUserRelations(GameUser source, GameUser target, OwnUserRelations newData)
    {
        DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(LevelCacheDurationSeconds);
        if (!this._cachedOwnUserRelations.ContainsKey(source.UserId)) 
            this._cachedOwnUserRelations[source.UserId] = [];

        this._cachedOwnUserRelations[source.UserId][target.UserId] = new(newData, expiresAt);
    }

    public void RemoveOwnUserRelations(GameUser source, GameUser target)
    {
        this._cachedOwnUserRelations.GetValueOrDefault(source.UserId)?.Remove(target.UserId);
    }

    public CachedReturn<OwnUserRelations> GetOwnUserRelations(GameUser source, GameUser target, GameDatabaseContext database)
    {
        CachedData<OwnUserRelations>? fromCache = this._cachedOwnUserRelations.GetValueOrDefault(source.UserId)?.GetValueOrDefault(target.UserId);

        if (this.HasCacheExpired(fromCache))
        {
            OwnUserRelations refreshed = new()
            {
                IsHearted = database.IsUserFavouritedByUser(target, source),
            };

            this.CacheOwnUserRelations(source, target, refreshed);
            return new(refreshed, true);
        }

        return new(fromCache!.Content, false);
    }

    public void UpdateUserHeartedStatusByUser(GameUser source, GameUser target, bool newValue, GameDatabaseContext database)
    {
        CachedReturn<OwnUserRelations> fromCache = this.GetOwnUserRelations(source, target, database);
        if (fromCache.WasRefreshed) return; // value is already up-to-date

        // dictionaries are already ensured to exist
        this._cachedOwnUserRelations[source.UserId][target.UserId].Content.IsHearted = newValue;
    }

    #endregion
    #region Own Level Relations

    public void CacheOwnLevelRelations(GameUser source, GameLevel target, OwnLevelRelations newData)
    {
        DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(LevelCacheDurationSeconds);
        if (!this._cachedOwnLevelRelations.ContainsKey(source.UserId)) 
            this._cachedOwnLevelRelations[source.UserId] = [];

        this._cachedOwnLevelRelations[source.UserId][target.LevelId] = new(newData, expiresAt);
    }

    public void RemoveOwnLevelRelations(GameUser source, GameLevel target)
    {
        this._cachedOwnLevelRelations.GetValueOrDefault(source.UserId)?.Remove(target.LevelId);
    }

    public CachedReturn<OwnLevelRelations> GetOwnLevelRelations(GameUser source, GameLevel target, GameDatabaseContext database)
    {
        CachedData<OwnLevelRelations>? fromCache = this._cachedOwnLevelRelations.GetValueOrDefault(source.UserId)?.GetValueOrDefault(target.LevelId);

        if (this.HasCacheExpired(fromCache))
        {
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

        return new(fromCache!.Content, false);
    }

    public void UpdateLevelHeartedStatusByUser(GameUser source, GameLevel target, bool newValue, GameDatabaseContext database)
    {
        CachedReturn<OwnLevelRelations> fromCache = this.GetOwnLevelRelations(source, target, database);
        if (fromCache.WasRefreshed) return; // value is already up-to-date

        // dictionaries are already ensured to exist
        this._cachedOwnLevelRelations[source.UserId][target.LevelId].Content.IsHearted = newValue;
    }

    public void UpdateLevelQueuedStatusByUser(GameUser source, GameLevel target, bool newValue, GameDatabaseContext database)
    {
        CachedReturn<OwnLevelRelations> fromCache = this.GetOwnLevelRelations(source, target, database);
        if (fromCache.WasRefreshed) return; // value is already up-to-date

        // dictionaries are already ensured to exist
        this._cachedOwnLevelRelations[source.UserId][target.LevelId].Content.IsQueued = newValue;
    }

    public void ClearQueueByUser(GameUser source)
    {
        foreach (KeyValuePair<int, CachedData<OwnLevelRelations>> relations in this._cachedOwnLevelRelations.GetValueOrDefault(source.UserId)?.ToArray() ?? [])
        {
            // will be refreshed later anyway; don't want too many DB calls here (e.g. user had 100 levels queued, this method would in that case send 600 DB queries)
            if (this.HasCacheExpired(relations.Value)) continue;

            relations.Value.Content.IsQueued = false;
            this._cachedOwnLevelRelations[source.UserId][relations.Key] = relations.Value;
        }
    }

    public void UpdateLevelRatingByUser(GameUser source, GameLevel target, int newRating, GameDatabaseContext database)
    {
        CachedReturn<OwnLevelRelations> fromCache = this.GetOwnLevelRelations(source, target, database);
        if (fromCache.WasRefreshed) return; // value is already up-to-date

        // dictionaries are already ensured to exist
        this._cachedOwnLevelRelations[source.UserId][target.LevelId].Content.LevelRating = newRating;
    }

    public void IncrementLevelTotalPlaysByUser(GameUser source, GameLevel target, int incrementor, GameDatabaseContext database)
    {
        CachedReturn<OwnLevelRelations> fromCache = this.GetOwnLevelRelations(source, target, database);
        if (fromCache.WasRefreshed) return; // value is already up-to-date

        // dictionaries are already ensured to exist
        this._cachedOwnLevelRelations[source.UserId][target.LevelId].Content.TotalPlayCount += incrementor;
    }

    public void IncrementLevelTotalCompletionsByUser(GameUser source, GameLevel target, int incrementor, GameDatabaseContext database)
    {
        CachedReturn<OwnLevelRelations> fromCache = this.GetOwnLevelRelations(source, target, database);
        if (fromCache.WasRefreshed) return; // value is already up-to-date

        // dictionaries are already ensured to exist
        this._cachedOwnLevelRelations[source.UserId][target.LevelId].Content.TotalCompletionCount += incrementor;
    }

    public void ResetLevelCompletionCountByUser(GameUser source)
    {
        foreach (KeyValuePair<int, CachedData<OwnLevelRelations>> relations in this._cachedOwnLevelRelations.GetValueOrDefault(source.UserId)?.ToArray() ?? [])
        {
            if (this.HasCacheExpired(relations.Value)) continue;

            relations.Value.Content.TotalCompletionCount = 0;
            this._cachedOwnLevelRelations[source.UserId][relations.Key] = relations.Value;
        }
    }

    public void IncrementLevelPhotosByUser(GameUser source, GameLevel target, int incrementor, GameDatabaseContext database)
    {
        CachedReturn<OwnLevelRelations> fromCache = this.GetOwnLevelRelations(source, target, database);
        if (fromCache.WasRefreshed) return; // value is already up-to-date

        // dictionaries are already ensured to exist
        this._cachedOwnLevelRelations[source.UserId][target.LevelId].Content.PhotoCount += incrementor;
    }

    public void ResetLevelPhotoCountsByUser(GameUser source)
    {
        foreach (KeyValuePair<int, CachedData<OwnLevelRelations>> relations in this._cachedOwnLevelRelations.GetValueOrDefault(source.UserId)?.ToArray() ?? [])
        {
            if (this.HasCacheExpired(relations.Value)) continue;

            relations.Value.Content.PhotoCount = 0;
            this._cachedOwnLevelRelations[source.UserId][relations.Key] = relations.Value;
        }
    }

    #endregion

    private bool HasCacheExpired<TData>(CachedData<TData>? cached)
        => cached == null || cached.ExpiresAt < this._time.TimeProvider.Now;
}