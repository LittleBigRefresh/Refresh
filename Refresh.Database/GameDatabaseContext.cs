using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Refresh.Common.Time;
using Refresh.Database.Configuration;
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
using System.Diagnostics;
using MongoDB.Bson;
using NotEnoughLogs;
using Refresh.Database.Models.Reports;
using Refresh.Database.Models.Statistics;
using Refresh.Database.Models.Workers;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Refresh.Database.Models.Moderation;

namespace Refresh.Database;

[SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
public partial class GameDatabaseContext : DbContext, IDatabaseContext
{
    private readonly IDateTimeProvider _time;
    private readonly IDatabaseConfig _dbConfig;
    private readonly Logger _logger;

    internal DbSet<GameUser> GameUsers { get; set; }
    internal DbSet<GameUserStatistics> GameUserStatistics { get; set; }
    internal DbSet<Token> Tokens { get; set; }
    internal DbSet<GameLevel> GameLevels { get; set; }
    internal DbSet<GameLevelStatistics> GameLevelStatistics { get; set; }
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
    internal DbSet<GameScore> GameScores { get; set; }
    internal DbSet<GameAsset> GameAssets { get; set; }
    internal DbSet<GameNotification> GameNotifications { get; set; }
    internal DbSet<GamePhoto> GamePhotos { get; set; }
    internal DbSet<GameIpVerificationRequest> GameIpVerificationRequests { get; set; }
    internal DbSet<GameAnnouncement> GameAnnouncements { get; set; }
    internal DbSet<QueuedRegistration> QueuedRegistrations { get; set; }
    internal DbSet<EmailVerificationCode> EmailVerificationCodes { get; set; }
    internal DbSet<RequestStatistics> RequestStatistics { get; set; }
    internal DbSet<GameContest> GameContests { get; set; }
    internal DbSet<AssetDependencyRelation> AssetDependencyRelations { get; set; }
    internal DbSet<GameReview> GameReviews { get; set; }
    internal DbSet<DisallowedUser> DisallowedUsers { get; set; }
    internal DbSet<RateReviewRelation> RateReviewRelations { get; set; }
    internal DbSet<TagLevelRelation> TagLevelRelations { get; set; }
    internal DbSet<GamePlaylist> GamePlaylists { get; set; }
    internal DbSet<GamePlaylistStatistics> GamePlaylistStatistics { get; set; }
    internal DbSet<LevelPlaylistRelation> LevelPlaylistRelations { get; set; }
    internal DbSet<SubPlaylistRelation> SubPlaylistRelations { get; set; }
    internal DbSet<FavouritePlaylistRelation> FavouritePlaylistRelations { get; set; }
    internal DbSet<GameUserVerifiedIpRelation> GameUserVerifiedIpRelations { get; set; }
    internal DbSet<GameChallenge> GameChallenges { get; set; }
    internal DbSet<GameChallengeScore> GameChallengeScores { get; set; }
    internal DbSet<PinProgressRelation> PinProgressRelations { get; set; }
    internal DbSet<ProfilePinRelation> ProfilePinRelations { get; set; }
    internal DbSet<GameSkillReward> GameSkillRewards { get; set; }
    internal DbSet<GriefReport> Reports { get; set; }
    internal DbSet<ReportPlayerRelation> ReportPlayersRelations { get; set; }
    internal DbSet<WorkerInfo> Workers { get; set; }
    internal DbSet<PersistentJobState> JobStates { get; set; }
    internal DbSet<GameLevelRevision> GameLevelRevisions { get; set; }
    internal DbSet<ModerationAction> ModerationActions { get; set; }
    
#pragma warning disable CS8618 // Non-nullable variable must contain a non-null value when exiting constructor. Consider declaring it as nullable.
    internal GameDatabaseContext(Logger logger, IDateTimeProvider time, IDatabaseConfig dbConfig)
    {
        this._logger = logger;
        this._time = time;
        this._dbConfig = dbConfig;
    }

    [Obsolete("For use by the `dotnet ef` tool only.", true)]
    public GameDatabaseContext() : this(new Logger(), new SystemDateTimeProvider(), new EmptyDatabaseConfig())
    {}

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        base.OnConfiguring(options);
        string connectionString = this._dbConfig.ConnectionString;
        if (this._dbConfig.PreferConnectionStringEnvironmentVariable)
        {
            string? envVarString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
            if (envVarString != null)
                connectionString = envVarString;
        }

        const LogLevel targetLevel = LogLevel.Warning;
        
        options.UseNpgsql(connectionString);
        options.LogTo(((_, level) => level >= targetLevel), (e) =>
        {
            NotEnoughLogs.LogLevel loggerLevel = e.LogLevel switch
            {
                LogLevel.Trace => NotEnoughLogs.LogLevel.Trace,
                LogLevel.Debug => NotEnoughLogs.LogLevel.Debug,
                LogLevel.Information => NotEnoughLogs.LogLevel.Info,
                LogLevel.Warning => NotEnoughLogs.LogLevel.Warning,
                LogLevel.Error => NotEnoughLogs.LogLevel.Error,
                LogLevel.Critical => NotEnoughLogs.LogLevel.Critical,
                LogLevel.None => NotEnoughLogs.LogLevel.Debug,
                _ => NotEnoughLogs.LogLevel.Trace,
            };

            const string codePrefix = "Database/";
            ReadOnlySpan<char> code = e.EventIdCode.AsSpan();

            Span<char> category = stackalloc char[code.Length + codePrefix.Length];
            codePrefix.CopyTo(category);
            code.CopyTo(category[codePrefix.Length..]);
            
            this._logger.Log(loggerLevel, category, e.ToString());
        });
        // options.LogTo(Console.WriteLine, LogLevel.Information);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder config)
    {
        config
            .Properties<ObjectId>()
            .HaveConversion<ObjectIdConverter>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Write(Action callback)
    {
        callback();
        this.SaveChanges();
    }

    private void RemoveAll<TClass>() where TClass : class
    {
        this.RemoveRange(this.Set<TClass>());
    }
    
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public void Refresh()
    {
        Debug.Assert(!this.ChangeTracker.HasChanges());
        this.ChangeTracker.Clear();
    }
}