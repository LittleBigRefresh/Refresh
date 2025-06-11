using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Refresh.Common.Time;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Activity;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Contests;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels.Challenges;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Notifications;
using Refresh.Database.Models.Photos;
using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Relations;
using Refresh.Database.Models;

namespace Refresh.Database;

[SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
public partial class GameDatabaseContext :
    #if !POSTGRES
    RealmDatabaseContext
    #else
    DbContext, IDatabaseContext
    #endif
{
    private static readonly object IdLock = new();

    private readonly IDateTimeProvider _time;
    
    #if !POSTGRES
    private RealmDbSet<GameUser> GameUsers => new(this._realm);
    private RealmDbSet<Token> Tokens => new(this._realm);
    private RealmDbSet<GameLevel> GameLevels => new(this._realm);
    private RealmDbSet<GameProfileComment> GameProfileComments => new(this._realm);
    private RealmDbSet<GameLevelComment> GameLevelComments => new(this._realm);
    private RealmDbSet<ProfileCommentRelation> ProfileCommentRelations => new(this._realm);
    private RealmDbSet<LevelCommentRelation> LevelCommentRelations => new(this._realm);
    private RealmDbSet<FavouriteLevelRelation> FavouriteLevelRelations => new(this._realm);
    private RealmDbSet<QueueLevelRelation> QueueLevelRelations => new(this._realm);
    private RealmDbSet<FavouriteUserRelation> FavouriteUserRelations => new(this._realm);
    private RealmDbSet<PlayLevelRelation> PlayLevelRelations => new(this._realm);
    private RealmDbSet<UniquePlayLevelRelation> UniquePlayLevelRelations => new(this._realm);
    private RealmDbSet<RateLevelRelation> RateLevelRelations => new(this._realm);
    private RealmDbSet<Event> Events => new(this._realm);
    private RealmDbSet<GameSubmittedScore> GameSubmittedScores => new(this._realm);
    private RealmDbSet<GameAsset> GameAssets => new(this._realm);
    private RealmDbSet<GameNotification> GameNotifications => new(this._realm);
    private RealmDbSet<GamePhoto> GamePhotos => new(this._realm);
    private RealmDbSet<GameIpVerificationRequest> GameIpVerificationRequests => new(this._realm);
    private RealmDbSet<GameAnnouncement> GameAnnouncements => new(this._realm);
    private RealmDbSet<QueuedRegistration> QueuedRegistrations => new(this._realm);
    private RealmDbSet<EmailVerificationCode> EmailVerificationCodes => new(this._realm);
    private RealmDbSet<RequestStatistics> RequestStatistics => new(this._realm);
    private RealmDbSet<SequentialIdStorage> SequentialIdStorage => new(this._realm);
    private RealmDbSet<GameContest> GameContests => new(this._realm);
    private RealmDbSet<AssetDependencyRelation> AssetDependencyRelations => new(this._realm);
    private RealmDbSet<GameReview> GameReviews => new(this._realm);
    private RealmDbSet<DisallowedUser> DisallowedUsers => new(this._realm);
    private RealmDbSet<RateReviewRelation> RateReviewRelations => new(this._realm);
    private RealmDbSet<TagLevelRelation> TagLevelRelations => new(this._realm);
    private RealmDbSet<GamePlaylist> GamePlaylists => new(this._realm);
    private RealmDbSet<LevelPlaylistRelation> LevelPlaylistRelations => new(this._realm);
    private RealmDbSet<SubPlaylistRelation> SubPlaylistRelations => new(this._realm);
    private RealmDbSet<FavouritePlaylistRelation> FavouritePlaylistRelations => new(this._realm);
    private RealmDbSet<GameUserVerifiedIpRelation> GameUserVerifiedIpRelations => new(this._realm);
    private RealmDbSet<GameChallenge> GameChallenges => new(this._realm);
    private RealmDbSet<GameChallengeScore> GameChallengeScores => new(this._realm);
    private RealmDbSet<PinProgressRelation> PinProgressRelations => new(this._realm);
    private RealmDbSet<ProfilePinRelation> ProfilePinRelations => new(this._realm);
    #else
    internal DbSet<GameUser> GameUsers { get; set; }
    internal DbSet<Token> Tokens { get; set; }
    internal DbSet<GameLevel> GameLevels { get; set; }
    internal DbSet<GameProfileComment> GameProfileComments { get; set; }
    internal DbSet<GameLevelComment> GameLevelComments { get; set; }
    internal DbSet<ProfileCommentRelation> ProfileCommentRelations { get; set; }
    internal DbSet<LevelCommentRelation> LevelCommentRelations { get; set; }
    internal DbSet<FavouriteLevelRelation> FavouriteLevelRelations { get; set; }
    internal DbSet<QueueLevelRelation> QueueLevelRelations { get; set; }
    internal DbSet<FavouriteUserRelation> FavouriteUserRelations { get; set; }
    internal DbSet<PlayLevelRelation> PlayLevelRelations { get; set; }
    internal DbSet<UniquePlayLevelRelation> UniquePlayLevelRelations { get; set; }
    internal DbSet<RateLevelRelation> RateLevelRelations { get; set; }
    internal DbSet<Event> Events { get; set; }
    internal DbSet<GameSubmittedScore> GameSubmittedScores { get; set; }
    internal DbSet<GameAsset> GameAssets { get; set; }
    internal DbSet<GameNotification> GameNotifications { get; set; }
    internal DbSet<GamePhoto> GamePhotos { get; set; }
    internal DbSet<GameIpVerificationRequest> GameIpVerificationRequests { get; set; }
    internal DbSet<GameAnnouncement> GameAnnouncements { get; set; }
    internal DbSet<QueuedRegistration> QueuedRegistrations { get; set; }
    internal DbSet<EmailVerificationCode> EmailVerificationCodes { get; set; }
    internal DbSet<RequestStatistics> RequestStatistics { get; set; }
    internal DbSet<SequentialIdStorage> SequentialIdStorage { get; set; }
    internal DbSet<GameContest> GameContests { get; set; }
    internal DbSet<AssetDependencyRelation> AssetDependencyRelations { get; set; }
    internal DbSet<GameReview> GameReviews { get; set; }
    internal DbSet<DisallowedUser> DisallowedUsers { get; set; }
    internal DbSet<RateReviewRelation> RateReviewRelations { get; set; }
    internal DbSet<TagLevelRelation> TagLevelRelations { get; set; }
    internal DbSet<GamePlaylist> GamePlaylists { get; set; }
    internal DbSet<LevelPlaylistRelation> LevelPlaylistRelations { get; set; }
    internal DbSet<SubPlaylistRelation> SubPlaylistRelations { get; set; }
    internal DbSet<FavouritePlaylistRelation> FavouritePlaylistRelations { get; set; }
    internal DbSet<GameUserVerifiedIpRelation> GameUserVerifiedIpRelations { get; set; }
    internal DbSet<GameChallenge> GameChallenges { get; set; }
    internal DbSet<GameChallengeScore> GameChallengeScores { get; set; }
    internal DbSet<PinProgressRelation> PinProgressRelations { get; set; }
    internal DbSet<ProfilePinRelation> ProfilePinRelations { get; set; }
    #endif
    
    internal GameDatabaseContext(IDateTimeProvider time)
    {
        this._time = time;
    }

#if POSTGRES
    [Obsolete("For use by the `dotnet ef` tool only.", true)]
    public GameDatabaseContext() : this(new SystemDateTimeProvider())
    {}

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        base.OnConfiguring(options);
        options.UseNpgsql();
    }
#endif

    private int GetOrCreateSequentialId<T>() where T : class, IRealmObject, ISequentialId
    {
        string name = typeof(T).Name;

        SequentialIdStorage? storage = this.SequentialIdStorage
            .FirstOrDefault(s => s.TypeName == name);

        if (storage != null)
        {
            storage.SequentialId += 1;
            return storage.SequentialId;
        }

        int objectCount = this.Set<T>().Count();
        
        storage = new SequentialIdStorage
        {
            TypeName = name,
        };

        if (objectCount != 0)
            storage.SequentialId = this.Set<T>()
                .AsEnumerable() // because realm.
                .Max(t => t.SequentialId) + 1;
        else
            storage.SequentialId = 0;

        // no need to do write block, this should only be called in a write transaction
        this.SequentialIdStorage.Add(storage);

        return storage.SequentialId;
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    private void AddSequentialObject<T>(T obj, IList<T>? list, Action? writtenCallback = null) where T : class, IRealmObject, ISequentialId
    {
        lock (IdLock)
        {
            this.Write(() =>
            {
                int newId = this.GetOrCreateSequentialId<T>() + 1;

                obj.SequentialId = newId;

                this.Add(obj);
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

    private void AddSequentialObject<T>(T obj, Action? writtenCallback) where T : class, IRealmObject, ISequentialId 
        => this.AddSequentialObject(obj, null, writtenCallback);
    
    private void AddSequentialObject<T>(T obj) where T : class, IRealmObject, ISequentialId 
        => this.AddSequentialObject(obj, null, null);

    #if !POSTGRES
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Write(Action callback)
    {
        // if already in a write transaction, include this within that write transaction
        // throws RealmInvalidTransactionException without this
        if (this._realm.IsInTransaction)
        {
            callback();
            return;
        }
        
        this._realm.Write(callback);
    }
    #else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Write(Action callback)
    {
        callback();
        this.SaveChanges();
    }
    #endif

    #if !POSTGRES
    private void RemoveAll<T>() where T : IRealmObject
    {
        this._realm.RemoveAll<T>();
    }

    private IQueryable<T> Set<T>() where T : IRealmObject
    {
        return this._realm.All<T>();
    }

    private void Add<T>(T obj) where T : IRealmObject
    {
        this._realm.Add(obj);
    }
    #else
    private void RemoveAll<TClass>() where TClass : class
    {
        this.RemoveRange(this.Set<TClass>());
    }
    
    [Obsolete("This has no effect in Postgres.")]
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public void Refresh() {}
    #endif
}