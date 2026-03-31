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
    // not using nested Dictionaries anymore because these flat Lists are easier to handle and less likely to throw exceptions (e.g. due to race conditions)
    // without being too inefficient compared to the former
    private readonly List<CachedData<string, GameAsset?>> _cachedAssetData = []; // hash -> data
    private readonly List<CachedData<int, List<GameSkillReward>>> _cachedSkillRewards = []; // level ID -> data
    private readonly List<CachedData<int, List<Tag>>> _cachedLevelTags = []; // level ID -> data

    private List<CachedRelationData<ObjectId, ObjectId, OwnUserRelations>> _cachedOwnUserRelations = []; // source user UUID -> target user UUID -> data
    private List<CachedRelationData<ObjectId, int, OwnLevelRelations>> _cachedOwnLevelRelations = []; // source user UUID -> target level ID -> data

    private const int LevelCacheDurationSeconds = 60 * 5;
    private const int AssetCacheDurationSeconds = 60 * 60; // GameAssets basically never change for now

    // TODO: unit tests for this
    public CacheService(Logger logger, TimeProviderService time) : base(logger)
    {
        this._time = time;
    }

    [Conditional("CACHE_DEBUG")]
    private void Log(LogLevel level, ReadOnlySpan<char> format, params object[] args)
    {
        this.Logger.Log(level, RefreshContext.CacheService, format, args);
    }

    public override void Initialize()
    {
        base.Initialize();

        // periodically automatically remove expired data if it hasn't been removed somehow else before,
        // to not keep unused data for a potentially undefined time
        Task expirationTask = new(async () =>
        {
            while(true)
            {
                await Task.Delay(1000 * 60 * 2);
                this.RemoveExpiredCache();
            }
        });

        expirationTask.Start();
    }

    private void RemoveExpiredCache()
    {
        this.Log(LogLevel.Debug, "Automatically removing expired cache...");
        DateTimeOffset now = this._time.TimeProvider.Now;
        int removed = 0;

        try
        {
            removed += this._cachedAssetData.RemoveAll(c => c.ExpiresAt < now);
            removed += this._cachedSkillRewards.RemoveAll(c => c.ExpiresAt < now);
            removed += this._cachedLevelTags.RemoveAll(c => c.ExpiresAt < now);
            removed += this._cachedOwnUserRelations.RemoveAll(c => c.ExpiresAt < now);
            removed += this._cachedOwnLevelRelations.RemoveAll(c => c.ExpiresAt < now);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when RemoveExpiredCache {ex.Message} {ex.Source} {ex.StackTrace}");
        }

        this.Log(LogLevel.Debug, $"Automatically removed {removed} items");
    }

    #region Assets
    public void CacheAsset(string hash, GameAsset? asset)
    {
        if (hash.StartsWith('g') || hash.IsBlankHash()) return;

        try
        {
            DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(AssetCacheDurationSeconds);
            this.Log(LogLevel.Debug, "Caching GameAsset {0}, it will expire at {1}", hash, expiresAt);
            this._cachedAssetData.Add(new(hash, asset, expiresAt));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when CacheAsset({hash}) {ex.Message} {ex.Source} {ex.StackTrace}");
        }
    }

    // currently not necessary to return these with CachedReturn...
    public GameAsset? GetAssetInfo(string hash, GameDatabaseContext database)
    {
        if (hash.StartsWith('g') || hash.IsBlankHash()) return null;

        try
        {
            this.Log(LogLevel.Debug, "Looking up GameAsset {0} in cache", hash);
            CachedData<string, GameAsset?>? fromCache = this._cachedAssetData.FirstOrDefault(c => c.Key == hash);

            if (fromCache == null)
            {
                this.Log(LogLevel.Debug, "Looking up GameAsset {0} in DB", hash);
                GameAsset? refreshed = database.GetAssetFromHash(hash);

                // Cache anyway, even if asset was not found in DB, for the same reason we're caching everything else
                // If the asset ever happens to be added to DB, it will be added to cache aswell by whatever is handling the upload/import anyway
                this.CacheAsset(hash, refreshed);
                return refreshed;
            }

            this.Log(LogLevel.Debug, "Found asset info {0} in cache, it will expire at {1}", hash, fromCache!.ExpiresAt);
            return fromCache!.Content;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when GetAssetInfo({hash}) {ex.Message} {ex.Source} {ex.StackTrace}");
            return database.GetAssetFromHash(hash);
        }
    }

    #endregion

    public void RemoveLevelData(GameLevel level)
    {
        try
        {
            this.RemoveSkillRewards(level);
            this.RemoveTags(level);
        }
        catch (Exception ex)
        {
            this.Log(LogLevel.Error, $"Exception when RemoveLevelData({level}) {ex.Message} {ex.Source} {ex.StackTrace}");
        }
    }

    #region Skill Rewards

    public void CacheSkillRewards(GameLevel level, List<GameSkillReward> rewards)
    {
        try
        {
            DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(LevelCacheDurationSeconds);
            this.Log(LogLevel.Debug, "Caching GameSkillRewards for level {0}, they will expire at {1}", level, expiresAt);
            this._cachedSkillRewards.Add(new(level.LevelId, rewards, expiresAt));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when CacheSkillRewards({level}) {ex.Message} {ex.Source} {ex.StackTrace}");
        }
    }

    public void RemoveSkillRewards(GameLevel level)
    {
        try
        {
            this.Log(LogLevel.Debug, "Removing GameSkillRewards cache for level {0}", level);
            this._cachedSkillRewards.RemoveAll(c => c.Key == level.LevelId);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when RemoveSkillRewards({level}) {ex.Message} {ex.Source} {ex.StackTrace}");
        }
    }

    public List<GameSkillReward> GetSkillRewards(GameLevel level, GameDatabaseContext database)
    {
        try
        {
            this.Log(LogLevel.Debug, "Looking up GameSkillRewards for level {0} in cache", level);
            CachedData<int, List<GameSkillReward>>? fromCache = this._cachedSkillRewards.FirstOrDefault(c => c.Key == level.LevelId);

            if (fromCache == null)
            {
                this.Log(LogLevel.Debug, "Looking up GameSkillRewards for level {0} in DB", level);
                List<GameSkillReward> refreshed = database.GetSkillRewardsForLevel(level).ToList();

                this.CacheSkillRewards(level, refreshed);
                return refreshed;
            }

            this.Log(LogLevel.Debug, "Found GameSkillRewards for level {0} in cache, they will expire at {1}", level, fromCache!.ExpiresAt);
            return fromCache!.Content;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when GetSkillRewards({level}) {ex.Message} {ex.Source} {ex.StackTrace}");
            return database.GetSkillRewardsForLevel(level).ToList();
        }
    }

    #endregion
    #region Tags

    public void CacheTags(GameLevel level, List<Tag> tags)
    {
        try
        {
            DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(LevelCacheDurationSeconds);
            this.Log(LogLevel.Debug, "Caching Tags for level {0}, they will expire at {1}", level, expiresAt);
            this._cachedLevelTags.Add(new(level.LevelId, tags, expiresAt));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when CacheTags({level}) {ex.Message} {ex.Source} {ex.StackTrace}");
        }
    }

    // Make service re-get tags the next time they're requested, incase a tag is added or removed.
    // Can't just lazily add or remove tag to/from the cache because they're grouped and sorted by frequency (data we don't have here).
    public void RemoveTags(GameLevel level)
    {
        try
        {
            this.Log(LogLevel.Debug, "Removing Tags cache for level {0}", level);
            this._cachedLevelTags.RemoveAll(c => c.Key == level.LevelId);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when RemoveTags({level}) {ex.Message} {ex.Source} {ex.StackTrace}");
        }
    }

    public List<Tag> GetTags(GameLevel level, GameDatabaseContext database)
    {
        try
        {
            this.Log(LogLevel.Debug, "Looking up Tags for level {0} in cache", level);
            CachedData<int, List<Tag>>? fromCache = this._cachedLevelTags.FirstOrDefault(c => c.Key == level.LevelId);

            if (fromCache == null)
            {
                this.Log(LogLevel.Debug, "Looking up Tags for level {0} in DB", level);
                List<Tag> refreshed = database.GetTagsForLevel(level).ToList();

                this.CacheTags(level, refreshed);
                return refreshed;
            }

            this.Log(LogLevel.Debug, "Found Tags for level {0} in cache, they will expire at {1}", level, fromCache!.ExpiresAt);
            return fromCache!.Content;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when GetTags({level}) {ex.Message} {ex.Source} {ex.StackTrace}");
            return database.GetTagsForLevel(level).ToList();
        }
    }

    #endregion

    #region Own User Relations

    public void CacheOwnUserRelations(GameUser source, GameUser target, OwnUserRelations newData)
    {
        try
        {
            DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(LevelCacheDurationSeconds);

            this.Log(LogLevel.Debug, "Caching OwnUserRelations by user {0} for user {1}, they will expire at {2}", source, target, expiresAt);
            this._cachedOwnUserRelations.Add(new(source.UserId, target.UserId, newData, expiresAt));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when CacheOwnUserRelations({source}, {target}) {ex.Message} {ex.Source} {ex.StackTrace}");
        }
    }

    public void RemoveOwnUserRelations(GameUser source, GameUser target)
    {
        try
        {
            this.Log(LogLevel.Debug, "Resetting OwnUserRelations by user {0}", source);
            this._cachedOwnUserRelations.RemoveAll(c => c.SourceKey == source.UserId && c.TargetKey == target.UserId);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when RemoveOwnUserRelations({source}, {target}) {ex.Message} {ex.Source} {ex.StackTrace}");
        }
    }

    private OwnUserRelations GetOwnUserRelationsFromDb(GameUser source, GameUser target, GameDatabaseContext database)
    {
        return new()
        {
            IsHearted = database.IsUserFavouritedByUser(target, source),
        };
    }

    public CachedReturn<OwnUserRelations> GetOwnUserRelations(GameUser source, GameUser target, GameDatabaseContext database)
    {
        try
        {
            this.Log(LogLevel.Debug, "Looking up OwnUserRelations by user {0} for user {0} in cache", source, target);
            CachedRelationData<ObjectId, ObjectId, OwnUserRelations>? fromCache = this._cachedOwnUserRelations.FirstOrDefault(c => c.SourceKey == source.UserId && c.TargetKey == target.UserId);

            if (fromCache == null)
            {
                this.Log(LogLevel.Debug, "Looking up OwnUserRelations by user {0} for user {1} in DB", source, target);
                OwnUserRelations refreshed = this.GetOwnUserRelationsFromDb(source, target, database);

                this.CacheOwnUserRelations(source, target, refreshed);
                return new(refreshed, true);
            }

            this.Log(LogLevel.Debug, "Found OwnUserRelations by user {0} for user {1} in cache, they will expire at {2}", source, target, fromCache!.ExpiresAt);
            return new(fromCache!.Content, false);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when GetOwnUserRelations({source}, {target}) {ex.Message} {ex.Source} {ex.StackTrace}");
            return new(this.GetOwnUserRelationsFromDb(source, target, database), true);
        }
    }

    public void UpdateUserHeartedStatusByUser(GameUser source, GameUser target, bool newValue, GameDatabaseContext database)
    {
        try
        {
            CachedReturn<OwnUserRelations> fromCache = this.GetOwnUserRelations(source, target, database);
            if (fromCache.WasRefreshed) return; // value is already up-to-date

            this.Log(LogLevel.Debug, "Lazily setting hearted status by user {0} for user {1} to {2}", source, target, newValue);
            this._cachedOwnUserRelations.First(c => c.SourceKey == source.UserId && c.TargetKey == target.UserId).Content.IsHearted = newValue;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when UpdateUserHeartedStatusByUser({source}, {target}, {newValue}) {ex.Message} {ex.Source} {ex.StackTrace}");
        }
    }

    #endregion
    #region Own Level Relations

    public void CacheOwnLevelRelations(GameUser source, GameLevel target, OwnLevelRelations newData)
    {
        try
        {
            DateTimeOffset expiresAt = this._time.TimeProvider.Now.AddSeconds(LevelCacheDurationSeconds);
            this.Log(LogLevel.Debug, "Caching OwnLevelRelations by user {0} for level {1}, they will expire at {2}", source, target, expiresAt);

            this._cachedOwnLevelRelations.Add(new(source.UserId, target.LevelId, newData, expiresAt));
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when CacheOwnLevelRelations({source}, {target}) {ex.Message} {ex.Source} {ex.StackTrace}");
        }
    }

    private OwnLevelRelations GetOwnLevelRelationsFromDb(GameUser source, GameLevel target, GameDatabaseContext database)
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

    public CachedReturn<OwnLevelRelations> GetOwnLevelRelations(GameUser source, GameLevel target, GameDatabaseContext database)
    {
        this.Log(LogLevel.Debug, "Looking up OwnLevelRelations by user {0} for level {0} in cache", source, target);
        try
        {
            CachedRelationData<ObjectId, int, OwnLevelRelations>? fromCache = this._cachedOwnLevelRelations.FirstOrDefault(c => c.SourceKey == source.UserId && c.TargetKey == target.LevelId);

            if (fromCache == null)
            {
                this.Log(LogLevel.Debug, "Looking up OwnLevelRelations by user {0} for level {1} in DB", source, target);
                OwnLevelRelations refreshed = this.GetOwnLevelRelationsFromDb(source, target, database);

                this.CacheOwnLevelRelations(source, target, refreshed);
                return new(refreshed, true);
            }

            this.Log(LogLevel.Debug, "Found OwnLevelRelations by user {0} for level {1} in cache, they will expire at {2}", source, target, fromCache!.ExpiresAt);
            return new(fromCache!.Content, false);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when GetOwnLevelRelations({source}, {target}) {ex.Message} {ex.Source} {ex.StackTrace}");
            return new(this.GetOwnLevelRelationsFromDb(source, target, database), true);
        }
    }

    public void UpdateLevelHeartedStatusByUser(GameUser source, GameLevel target, bool newValue, GameDatabaseContext database)
    {
        try
        {
            CachedReturn<OwnLevelRelations> fromCache = this.GetOwnLevelRelations(source, target, database);
            if (fromCache.WasRefreshed) return; // value is already up-to-date

            this.Log(LogLevel.Debug, "Lazily setting hearted status by user {0} for level {1} to {2}", source, target, newValue);
            this._cachedOwnLevelRelations.First(c => c.SourceKey == source.UserId && c.TargetKey == target.LevelId).Content.IsHearted = newValue;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when UpdateLevelHeartedStatusByUser({source}, {target}, {newValue}) {ex.Message} {ex.Source} {ex.StackTrace}");
        }
    }

    public void UpdateLevelQueuedStatusByUser(GameUser source, GameLevel target, bool newValue, GameDatabaseContext database)
    {
        try
        {
            CachedReturn<OwnLevelRelations> fromCache = this.GetOwnLevelRelations(source, target, database);
            if (fromCache.WasRefreshed) return; // value is already up-to-date

            this.Log(LogLevel.Debug, "Lazily setting queued status by user {0} for level {1} to {2}", source, target, newValue);
            this._cachedOwnLevelRelations.First(c => c.SourceKey == source.UserId && c.TargetKey == target.LevelId).Content.IsQueued = newValue;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when UpdateLevelQueuedStatusByUser({source}, {target}, {newValue}) {ex.Message} {ex.Source} {ex.StackTrace}");
        }
    }

    public void ClearQueueByUser(GameUser source)
    {
        this.Log(LogLevel.Debug, "Resetting queued stati by user {0}", source);
        DateTimeOffset now = this._time.TimeProvider.Now;
        try
        {
            foreach (CachedRelationData<ObjectId, int, OwnLevelRelations> relations in this._cachedOwnLevelRelations.Where(c => c.SourceKey == source.UserId))
            {
                // will be refreshed later anyway; don't want too many DB calls here (e.g. user had 100 levels queued, this method would in that case send 600 DB queries)
                if (relations.ExpiresAt < now) continue;

                this.Log(LogLevel.Debug, "Resetting queued status by user {0} for level {1}", source, relations.TargetKey);
                relations.Content.IsQueued = false;
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when ClearQueueByUser({source}) {ex.Message} {ex.Source} {ex.StackTrace}");
        }
    }

    public void UpdateLevelRatingByUser(GameUser source, GameLevel target, int newRating, GameDatabaseContext database)
    {
        try
        {
            CachedReturn<OwnLevelRelations> fromCache = this.GetOwnLevelRelations(source, target, database);
            if (fromCache.WasRefreshed) return; // value is already up-to-date

            this.Log(LogLevel.Debug, "Lazily setting level rating by user {0} for level {1} to {2}", source, target, newRating);
            this._cachedOwnLevelRelations.First(c => c.SourceKey == source.UserId && c.TargetKey == target.LevelId).Content.LevelRating = newRating;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when UpdateLevelRatingByUser({source}, {target}, {newRating}) {ex.Message} {ex.Source} {ex.StackTrace}");
        }
    }

    public void IncrementLevelTotalPlaysByUser(GameUser source, GameLevel target, int incrementor, GameDatabaseContext database)
    {
        try
        {
            CachedReturn<OwnLevelRelations> fromCache = this.GetOwnLevelRelations(source, target, database);
            if (fromCache.WasRefreshed) return; // value is already up-to-date

            this.Log(LogLevel.Debug, "Lazily incrementing total plays by user {0} for level {1} by {2}", source, target, incrementor);
            this._cachedOwnLevelRelations.First(c => c.SourceKey == source.UserId && c.TargetKey == target.LevelId).Content.TotalPlayCount += incrementor;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when IncrementLevelTotalPlaysByUser({source}, {target}, {incrementor}) {ex.Message} {ex.Source} {ex.StackTrace}");
        }
    }

    public void IncrementLevelTotalCompletionsByUser(GameUser source, GameLevel target, int incrementor, GameDatabaseContext database)
    {
        try
        {
            CachedReturn<OwnLevelRelations> fromCache = this.GetOwnLevelRelations(source, target, database);
            if (fromCache.WasRefreshed) return; // value is already up-to-date

            this.Log(LogLevel.Debug, "Lazily incrementing total completions by user {0} for level {1} by {2}", source, target, incrementor);
            this._cachedOwnLevelRelations.First(c => c.SourceKey == source.UserId && c.TargetKey == target.LevelId).Content.TotalCompletionCount += incrementor;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when IncrementLevelTotalCompletionsByUser({source}, {target}, {incrementor}) {ex.Message} {ex.Source} {ex.StackTrace}");
        }
    }

    public void ResetLevelCompletionCountByUser(GameUser source)
    {
        this.Log(LogLevel.Debug, "Resetting total completions by user {0}", source);
        DateTimeOffset now = this._time.TimeProvider.Now;
        try
        {
            foreach (CachedRelationData<ObjectId, int, OwnLevelRelations> relations in this._cachedOwnLevelRelations.Where(c => c.SourceKey == source.UserId))
            {
                if (relations.ExpiresAt < now) continue;

                this.Log(LogLevel.Debug, "Resetting total completions by user {0} for level {1}", source, relations.TargetKey);
                relations.Content.TotalCompletionCount = 0;
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when ResetLevelCompletionCountByUser({source}) {ex.Message} {ex.Source} {ex.StackTrace}");
        }
    }

    public void IncrementLevelPhotosByUser(GameUser source, GameLevel target, int incrementor, GameDatabaseContext database)
    {
        try
        {
            CachedReturn<OwnLevelRelations> fromCache = this.GetOwnLevelRelations(source, target, database);
            if (fromCache.WasRefreshed) return; // value is already up-to-date

            this.Log(LogLevel.Debug, "Lazily incrementing total photos by user {0} for level {1} by {2}", source, target, incrementor);
            this._cachedOwnLevelRelations.First(c => c.SourceKey == source.UserId && c.TargetKey == target.LevelId).Content.PhotoCount += incrementor;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when IncrementLevelPhotosByUser({source}, {target}, {incrementor}) {ex.Message} {ex.Source} {ex.StackTrace}");
        }
    }

    public void ResetLevelPhotoCountsByUser(GameUser source)
    {
        this.Log(LogLevel.Debug, "Resetting total photos by user {0}", source);
        DateTimeOffset now = this._time.TimeProvider.Now;
        try
        {
            foreach (CachedRelationData<ObjectId, int, OwnLevelRelations> relations in this._cachedOwnLevelRelations.Where(c => c.SourceKey == source.UserId))
            {
                if (relations.ExpiresAt < now) continue;

                this.Log(LogLevel.Debug, "Resetting total photos by user {0} for level {1}", source, relations.TargetKey);
                relations.Content.PhotoCount = 0;
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(RefreshContext.CacheService, $"Exception when ResetLevelPhotoCountsByUser({source}) {ex.Message} {ex.Source} {ex.StackTrace}");
        }
    }

    #endregion
}