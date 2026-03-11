using Bunkum.Core;
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
    private readonly Dictionary<string, CachedData<GameAsset>> _cachedAssetData = []; // hash -> data
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
        DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(AssetCacheDurationSeconds);
        this._cachedAssetData[hash] = new()
        {
            Cached = asset,
            ExpiresAt = expiresAt
        };
        this.Logger.LogDebug(BunkumCategory.UserPhotos, $"CacheAsset {hash} - will expire in {expiresAt} Ä");
    }

    public GameAsset? GetAssetInfo(string hash, GameDatabaseContext database)
    {
        CachedData<GameAsset>? cached = this._cachedAssetData.GetValueOrDefault(hash);
        this.Logger.LogDebug(BunkumCategory.UserPhotos, $"GetAssetInfo {hash} - cached {cached} expires in {cached?.ExpiresAt} Ä");

        if (this.HasCacheExpired(cached))
        {
            GameAsset? refreshed = database.GetAssetFromHash(hash);
            this.Logger.LogDebug(BunkumCategory.UserPhotos, $"GetAssetInfo {hash} - gotten from DB {refreshed} Ä");

            // Cache anyway, even if asset was not found in DB, for the same reason we're caching everything else
            // If the asset ever happens to be added to DB, it will be added to cache aswell by whatever is handling the upload/import anyway
            this.CacheAsset(hash, refreshed);
            return refreshed;
        }

        return cached!.Cached;
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
        this._cachedSkillRewards[level.LevelId] = new()
        {
            Cached = rewards,
            ExpiresAt = expiresAt
        };
        this.Logger.LogDebug(BunkumCategory.UserPhotos, $"CacheSkillRewards {level.LevelId} - will expire in {expiresAt} Ä");
    }

    public void RemoveSkillRewards(GameLevel level)
    {
        this._cachedSkillRewards.Remove(level.LevelId);
    }

    public List<GameSkillReward> GetSkillRewards(GameLevel level, GameDatabaseContext database)
    {
        CachedData<List<GameSkillReward>>? cached = this._cachedSkillRewards.GetValueOrDefault(level.LevelId);
        this.Logger.LogDebug(BunkumCategory.UserPhotos, $"GetSkillRewards {level.LevelId} - cached {cached} expires in {cached?.ExpiresAt} Ä");

        if (this.HasCacheExpired(cached))
        {
            List<GameSkillReward> refreshed = database.GetSkillRewardsForLevel(level).ToList();
            this.Logger.LogDebug(BunkumCategory.UserPhotos, $"GetSkillRewards {level.LevelId} - gotten from DB {refreshed} Ä");

            this.CacheSkillRewards(level, refreshed);
            return refreshed;
        }

        return cached!.Cached ?? []; // should never be null, but incase
    }

    #endregion
    #region Tags

    public void CacheTags(GameLevel level, List<Tag> tags)
    {
        DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(LevelCacheDurationSeconds);
        this._cachedLevelTags[level.LevelId] = new()
        {
            Cached = tags,
            ExpiresAt = expiresAt
        };
        this.Logger.LogDebug(BunkumCategory.UserPhotos, $"CacheTags {level.LevelId} - will expire in {expiresAt} Ä");
    }

    // Make service re-get tags the next time they're requested, incase a tag is added or removed.
    // Can't just lazily add or remove tag to/from the cache because they're grouped and sorted by frequency (data we don't have here).
    public void RemoveTags(GameLevel level)
    {
        this._cachedLevelTags.Remove(level.LevelId);
    }

    public List<Tag> GetTags(GameLevel level, GameDatabaseContext database)
    {
        CachedData<List<Tag>>? cached = this._cachedLevelTags.GetValueOrDefault(level.LevelId);
        this.Logger.LogDebug(BunkumCategory.UserPhotos, $"GetTags {level.LevelId} - cached {cached} expires in {cached?.ExpiresAt} Ä");

        if (this.HasCacheExpired(cached))
        {
            List<Tag> refreshed = database.GetTagsForLevel(level).ToList();
            this.Logger.LogDebug(BunkumCategory.UserPhotos, $"GetTags {level.LevelId} - gotten from DB {refreshed} Ä");

            this.CacheTags(level, refreshed);
            return refreshed;
        }

        return cached!.Cached ?? [];
    }

    #endregion

    #region Own User Relations

    private OwnUserRelations CreateUserRelations(GameUser source, GameUser target, GameDatabaseContext database)
    {
        return new()
        {
            IsHearted = database.IsUserFavouritedByUser(target, source),
        };
    }

    public void CacheOwnUserRelations(GameUser source, GameUser target, OwnUserRelations newData)
    {
        DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(LevelCacheDurationSeconds);
        this._cachedOwnUserRelations[source.UserId][target.UserId] = new()
        {
            Cached = newData,
            ExpiresAt = expiresAt
        };
        this.Logger.LogDebug(BunkumCategory.UserPhotos, $"CacheOwnUserRelations s {source.UserId} t {target.UserId} - will expire in {expiresAt} Ä");
    }

    // TODO: minimal chance of wrong cached value if refreshed from DB
    public void UpdateUserHeart(GameUser source, GameUser target, bool newValue, GameDatabaseContext database)
    {
        OwnUserRelations existing = this.GetOwnUserRelations(source, target, database);
        existing.IsHearted = newValue;
        this.CacheOwnUserRelations(source, target, existing);
    }

    public OwnUserRelations GetOwnUserRelations(GameUser source, GameUser target, GameDatabaseContext database)
    {
        CachedData<OwnUserRelations>? cached = this._cachedOwnUserRelations.GetValueOrDefault(source.UserId)?.GetValueOrDefault(target.UserId);
        this.Logger.LogDebug(BunkumCategory.UserPhotos, $"GetOwnUserRelations s {source.UserId} t {target.UserId} - cached {cached} expires in {cached?.ExpiresAt} Ä");

        if (this.HasCacheExpired(cached))
        {
            OwnUserRelations refreshed = this.CreateUserRelations(source, target, database);
            this.Logger.LogDebug(BunkumCategory.UserPhotos, $"GetOwnUserRelations s {source.UserId} t {target.UserId} - gotten from DB {refreshed} Ä");

            this.CacheOwnUserRelations(source, target, refreshed);
            return refreshed;
        }

        return cached!.Cached!;
    }

    #endregion
    #region Own Level Relations

    private OwnLevelRelations CreateLevelRelations(GameUser source, GameLevel target, GameDatabaseContext database)
    {
        return new()
        {
            IsHearted = database.IsLevelFavouritedByUser(target, source),
            IsQueued = database.IsLevelQueuedByUser(target, source),
            LevelRating = (int?)database.GetRatingByUser(target, source) ?? 0,
            TotalPlayCount = database.GetTotalPlaysForLevelByUser(target, source),
            TotalCompletionCount = database.GetTotalCompletionsForLevelByUser(target, source),
            PhotoCount = database.GetTotalPhotosInLevelByUser(target, source),
        };
    }

    public void UpdateLevelHeart(GameUser source, GameLevel target, bool newValue, GameDatabaseContext database)
    {
        OwnLevelRelations existing = this.GetOwnLevelRelations(source, target, database);
        existing.IsHearted = newValue;
        this.CacheOwnLevelRelations(source, target, existing);
    }

    public void UpdateLevelQueue(GameUser source, GameLevel target, bool newValue, GameDatabaseContext database)
    {
        OwnLevelRelations existing = this.GetOwnLevelRelations(source, target, database);
        existing.IsHearted = newValue;
        this.CacheOwnLevelRelations(source, target, existing);
    }

    public void DequeueAllLevels(GameUser source, GameDatabaseContext database)
    {
        foreach (KeyValuePair<int, CachedData<OwnLevelRelations>> relations in this._cachedOwnLevelRelations.GetValueOrDefault(source.UserId)?.ToArray() ?? [])
        {
            // will be refreshed later anyway; don't want too many DB calls here (e.g. user had 100 levels queued, this method would in that case send 600 DB queries)
            if (this.HasCacheExpired(relations.Value)) continue;

            relations.Value.Cached!.IsQueued = false;
            this._cachedOwnLevelRelations[source.UserId][relations.Key] = relations.Value;
        }
    }

    public void UpdateLevelRating(GameUser source, GameLevel target, int newRating, GameDatabaseContext database)
    {
        OwnLevelRelations existing = this.GetOwnLevelRelations(source, target, database);
        existing.LevelRating = newRating;
        this.CacheOwnLevelRelations(source, target, existing);
    }

    public void IncrementLevelTotalPlays(GameUser source, GameLevel target, int incrementor, GameDatabaseContext database)
    {
        OwnLevelRelations existing = this.GetOwnLevelRelations(source, target, database);
        existing.TotalPlayCount += incrementor;
        this.CacheOwnLevelRelations(source, target, existing);
    }

    public void IncrementLevelTotalCompletions(GameUser source, GameLevel target, int incrementor, GameDatabaseContext database)
    {
        OwnLevelRelations existing = this.GetOwnLevelRelations(source, target, database);
        existing.TotalCompletionCount += incrementor;
        this.CacheOwnLevelRelations(source, target, existing);
    }

    public void IncrementLevelPhotos(GameUser source, GameLevel target, int incrementor, GameDatabaseContext database)
    {
        OwnLevelRelations existing = this.GetOwnLevelRelations(source, target, database);
        existing.PhotoCount += incrementor;
        this.CacheOwnLevelRelations(source, target, existing);
    }

    public void CacheOwnLevelRelations(GameUser source, GameLevel target, OwnLevelRelations newData)
    {
        DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(LevelCacheDurationSeconds);
        this._cachedOwnLevelRelations[source.UserId][target.LevelId] = new()
        {
            Cached = newData,
            ExpiresAt = expiresAt
        };
        this.Logger.LogDebug(BunkumCategory.UserPhotos, $"CacheOwnLevelRelations s {source.UserId} t {target.LevelId} - will expire in {expiresAt} Ä");
    }

    public OwnLevelRelations GetOwnLevelRelations(GameUser source, GameLevel target, GameDatabaseContext database)
    {
        CachedData<OwnLevelRelations>? cached = this._cachedOwnLevelRelations.GetValueOrDefault(source.UserId)?.GetValueOrDefault(target.LevelId);
        this.Logger.LogDebug(BunkumCategory.UserPhotos, $"GetOwnLevelRelations s {source.UserId} t {target.LevelId} - cached {cached} expires in {cached?.ExpiresAt} Ä");

        if (this.HasCacheExpired(cached))
        {
            OwnLevelRelations refreshed = this.CreateLevelRelations(source, target, database);
            this.Logger.LogDebug(BunkumCategory.UserPhotos, $"GetOwnLevelRelations s {source.UserId} t {target.LevelId} - gotten from DB {refreshed} plays {refreshed.TotalPlayCount} Ä");

            this.CacheOwnLevelRelations(source, target, refreshed);
            return refreshed;
        }

        return cached!.Cached!;
    }

    #endregion

    private bool HasCacheExpired<TData>(CachedData<TData>? cached)
        => cached == null || cached.ExpiresAt < this._time.TimeProvider.Now;
}