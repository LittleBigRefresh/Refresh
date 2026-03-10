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
    private readonly Dictionary<string, CachedAssetData> _cachedAssetData = []; // hash -> data
    private readonly Dictionary<int, CachedSkillRewards> _cachedSkillRewards = []; // level ID -> data
    private readonly Dictionary<int, CachedLevelTags> _cachedLevelTags = []; // level ID -> data

    // TODO: maybe save these in DB aswell?
    private readonly Dictionary<ObjectId, Dictionary<ObjectId, CachedOwnUserRelations>> _cachedOwnUserRelations = []; // source user UUID -> target user UUID -> data
    private readonly Dictionary<ObjectId, Dictionary<int, CachedOwnLevelRelations>> _cachedOwnLevelRelations = []; // source user UUID -> level ID -> data

    private const int LevelCacheDurationSeconds = 60 * 10; // only caching skill rewards and tags for now
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
            Asset = asset,
            ExpiresAt = expiresAt
        };
        this.Logger.LogDebug(BunkumCategory.UserPhotos, $"CacheAsset {hash} - will expire in {expiresAt} Ä");
    }

    public GameAsset? GetAssetInfo(string hash, GameDatabaseContext database)
    {
        CachedAssetData? cached = this._cachedAssetData.GetValueOrDefault(hash);
        this.Logger.LogDebug(BunkumCategory.UserPhotos, $"GetAssetInfo {hash} - cached {cached} expires in {cached?.ExpiresAt} Ä");

        if (cached == null || cached.ExpiresAt < this._time.TimeProvider.Now)
        {
            GameAsset? refreshed = database.GetAssetFromHash(hash);
            this.Logger.LogDebug(BunkumCategory.UserPhotos, $"GetAssetInfo {hash} - gotten from DB {refreshed} Ä");
            if (refreshed == null) return null;

            this.CacheAsset(hash, refreshed);
            return refreshed;
        }

        return cached.Asset;
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
            SkillRewards = rewards,
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
        CachedSkillRewards? cached = this._cachedSkillRewards.GetValueOrDefault(level.LevelId);
        this.Logger.LogDebug(BunkumCategory.UserPhotos, $"GetSkillRewards {level.LevelId} - cached {cached} expires in {cached?.ExpiresAt} Ä");

        if (cached == null || cached.ExpiresAt < this._time.TimeProvider.Now)
        {
            List<GameSkillReward> refreshed = database.GetSkillRewardsForLevel(level).ToList();
            this.Logger.LogDebug(BunkumCategory.UserPhotos, $"GetSkillRewards {level.LevelId} - gotten from DB {refreshed} Ä");

            this.CacheSkillRewards(level, refreshed);
            return refreshed;
        }

        return cached.SkillRewards;
    }

    #endregion
    #region Tags

    public void CacheTags(GameLevel level, List<Tag> tags)
    {
        DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(LevelCacheDurationSeconds);
        this._cachedLevelTags[level.LevelId] = new()
        {
            Tags = tags,
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
        CachedLevelTags? cached = this._cachedLevelTags.GetValueOrDefault(level.LevelId);
        this.Logger.LogDebug(BunkumCategory.UserPhotos, $"GetTags {level.LevelId} - cached {cached} expires in {cached?.ExpiresAt} Ä");

        if (cached == null || cached.ExpiresAt < this._time.TimeProvider.Now)
        {
            List<Tag> refreshed = database.GetTagsForLevel(level).ToList();
            this.Logger.LogDebug(BunkumCategory.UserPhotos, $"GetTags {level.LevelId} - gotten from DB {refreshed} Ä");

            this.CacheTags(level, refreshed);
            return refreshed;
        }

        return cached.Tags;
    }

    #endregion

    #region Own User Relations

    public void CacheOwnUserRelations(GameUser source, GameUser target, OwnUserRelations newData)
    {
        DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(LevelCacheDurationSeconds);
        this._cachedOwnUserRelations[source.UserId][target.UserId] = new()
        {
            Relations = newData,
            ExpiresAt = expiresAt
        };
        this.Logger.LogDebug(BunkumCategory.UserPhotos, $"CacheOwnUserRelations s {source.UserId} t {target.UserId} - will expire in {expiresAt} Ä");
    }
    public OwnUserRelations GetOwnUserRelations(GameUser source, GameUser target, GameDatabaseContext database)
    {
        CachedOwnUserRelations? cached = this._cachedOwnUserRelations.GetValueOrDefault(source.UserId)?.GetValueOrDefault(target.UserId);
        this.Logger.LogDebug(BunkumCategory.UserPhotos, $"GetOwnUserRelations s {source.UserId} t {target.UserId} - cached {cached} expires in {cached?.ExpiresAt} Ä");

        if (cached == null || cached.ExpiresAt < this._time.TimeProvider.Now)
        {
            OwnUserRelations refreshed = new()
            {
                IsHearted = database.IsUserFavouritedByUser(target, source),
            };
            this.Logger.LogDebug(BunkumCategory.UserPhotos, $"GetOwnUserRelations s {source.UserId} t {target.UserId} - gotten from DB {refreshed} Ä");

            this.CacheOwnUserRelations(source, target, refreshed);
            return refreshed;
        }

        return cached.Relations;
    }

    #endregion
    #region Own Level Relations

    public void CacheOwnLevelRelations(GameUser source, GameLevel target, OwnLevelRelations newData)
    {
        DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(LevelCacheDurationSeconds);
        this._cachedOwnLevelRelations[source.UserId][target.LevelId] = new()
        {
            Relations = newData,
            ExpiresAt = expiresAt
        };
        this.Logger.LogDebug(BunkumCategory.UserPhotos, $"CacheOwnLevelRelations s {source.UserId} t {target.LevelId} - will expire in {expiresAt} Ä");
    }

    public OwnLevelRelations GetOwnLevelRelations(GameUser source, GameLevel target, GameDatabaseContext database)
    {
        CachedOwnLevelRelations? cached = this._cachedOwnLevelRelations.GetValueOrDefault(source.UserId)?.GetValueOrDefault(target.LevelId);
        this.Logger.LogDebug(BunkumCategory.UserPhotos, $"GetOwnLevelRelations s {source.UserId} t {target.LevelId} - cached {cached} expires in {cached?.ExpiresAt} Ä");

        if (cached == null || cached.ExpiresAt < this._time.TimeProvider.Now)
        {
            OwnLevelRelations refreshed = new()
            {
                IsHearted = database.IsLevelFavouritedByUser(target, source),
                IsQueued = database.IsLevelQueuedByUser(target, source),
                LevelRating = (int?)database.GetRatingByUser(target, source) ?? 0,
                TotalPlayCount = database.GetTotalPlaysForLevelByUser(target, source),
                TotalCompletionCount = database.GetTotalCompletionsForLevelByUser(target, source),
                PhotoCount = database.GetTotalPhotosInLevelByUser(target, source)
            };
            this.Logger.LogDebug(BunkumCategory.UserPhotos, $"GetOwnLevelRelations s {source.UserId} t {target.LevelId} - gotten from DB {refreshed} Ä");

            this.CacheOwnLevelRelations(source, target, refreshed);
            return refreshed;
        }

        return cached.Relations;
    }

    #endregion
}