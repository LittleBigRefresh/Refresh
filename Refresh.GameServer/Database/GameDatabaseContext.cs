using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Realms;
using Bunkum.RealmDatabase;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Assets;
using Refresh.GameServer.Types.Comments;
using Refresh.GameServer.Types.Contests;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Levels.SkillRewards;
using Refresh.GameServer.Types.Notifications;
using Refresh.GameServer.Types.Photos;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Database;

[SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
public partial class GameDatabaseContext : RealmDatabaseContext
{
    private static readonly object IdLock = new();

    private readonly IDateTimeProvider _time;
    
    private IQueryable<GameUser> GameUsers => this._realm.All<GameUser>();
    private IQueryable<Token> Tokens => this._realm.All<Token>();
    private IQueryable<GameLevel> GameLevels => this._realm.All<GameLevel>();
    private IQueryable<GameComment> GameComments => this._realm.All<GameComment>();
    private IQueryable<CommentRelation> CommentRelations => this._realm.All<CommentRelation>();
    private IQueryable<FavouriteLevelRelation> FavouriteLevelRelations => this._realm.All<FavouriteLevelRelation>();
    private IQueryable<QueueLevelRelation> QueueLevelRelations => this._realm.All<QueueLevelRelation>();
    private IQueryable<FavouriteUserRelation> FavouriteUserRelations => this._realm.All<FavouriteUserRelation>();
    private IQueryable<PlayLevelRelation> PlayLevelRelations => this._realm.All<PlayLevelRelation>();
    private IQueryable<UniquePlayLevelRelation> UniquePlayLevelRelations => this._realm.All<UniquePlayLevelRelation>();
    private IQueryable<RateLevelRelation> RateLevelRelations => this._realm.All<RateLevelRelation>();
    private IQueryable<Event> Events => this._realm.All<Event>();
    private IQueryable<GameSubmittedScore> GameSubmittedScores => this._realm.All<GameSubmittedScore>();
    private IQueryable<GameAsset> GameAssets => this._realm.All<GameAsset>();
    private IQueryable<GameNotification> GameNotifications => this._realm.All<GameNotification>();
    private IQueryable<GamePhoto> GamePhotos => this._realm.All<GamePhoto>();
    private IQueryable<GameAnnouncement> GameAnnouncements => this._realm.All<GameAnnouncement>();
    private IQueryable<QueuedRegistration> QueuedRegistrations => this._realm.All<QueuedRegistration>();
    private IQueryable<EmailVerificationCode> EmailVerificationCodes => this._realm.All<EmailVerificationCode>();
    private IQueryable<RequestStatistics> RequestStatistics => this._realm.All<RequestStatistics>();
    private IQueryable<SequentialIdStorage> SequentialIdStorage => this._realm.All<SequentialIdStorage>();
    private IQueryable<GameContest> GameContests => this._realm.All<GameContest>();
    private IQueryable<AssetDependencyRelation> AssetDependencyRelations => this._realm.All<AssetDependencyRelation>();
    private IQueryable<GameReview> GameReviews => this._realm.All<GameReview>();
    private IQueryable<DisallowedUser> DisallowedUsers => this._realm.All<DisallowedUser>();

    internal GameDatabaseContext(IDateTimeProvider time)
    {
        this._time = time;
    }

    private int GetOrCreateSequentialId<T>() where T : IRealmObject, ISequentialId
    {
        string name = typeof(T).Name;

        SequentialIdStorage? storage = this.SequentialIdStorage
            .FirstOrDefault(s => s.TypeName == name);

        if (storage != null)
        {
            storage.SequentialId += 1;
            return storage.SequentialId;
        }
        
        storage = new SequentialIdStorage
        {
            TypeName = name,
            SequentialId = this.SequentialIdStorage.Count() * 2, // skip over a bunch of ids incase table is broken
        };

        // no need to do write block, this should only be called in a write transaction
        this._realm.Add(storage);

        return storage.SequentialId;
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    private void AddSequentialObject<T>(T obj, IList<T>? list, Action? writtenCallback = null) where T : IRealmObject, ISequentialId
    {
        lock (IdLock)
        {
            this.Write(() =>
            {
                int newId = this.GetOrCreateSequentialId<T>() + 1;

                obj.SequentialId = newId;

                this._realm.Add(obj);
                if(list == null) writtenCallback?.Invoke();
            });
        }
        
        // Two writes are necessary here for some unexplainable reason
        // We've already set a SequentialId so we can be outside the lock at this stage
        if (list != null)
        {
            this.Write(() =>
            {
                list.Add(obj);
                writtenCallback?.Invoke();
            });
        }
    }

    private void AddSequentialObject<T>(T obj, Action? writtenCallback) where T : IRealmObject, ISequentialId 
        => this.AddSequentialObject(obj, null, writtenCallback);
    
    private void AddSequentialObject<T>(T obj) where T : IRealmObject, ISequentialId 
        => this.AddSequentialObject(obj, null, null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Write(Action callback)
    {
        this._realm.Write(callback);
    }
}