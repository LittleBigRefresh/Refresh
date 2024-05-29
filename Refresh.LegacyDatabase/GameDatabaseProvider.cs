using Bunkum.RealmDatabase;
using Realms;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Assets;
using Refresh.GameServer.Types.Comments;
using Refresh.GameServer.Types.Contests;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Levels.SkillRewards;
using Refresh.GameServer.Types.Notifications;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.Report;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;
using GamePhoto = Refresh.GameServer.Types.Photos.GamePhoto;
using GamePhotoSubject = Refresh.GameServer.Types.Photos.GamePhotoSubject;

namespace Refresh.LegacyDatabase;

public class GameDatabaseProvider : RealmDatabaseProvider<GameDatabaseContext>
{
    private readonly IDateTimeProvider _time;
    
    public GameDatabaseProvider()
    {
        this._time = new SystemDateTimeProvider();
    }

    protected GameDatabaseProvider(IDateTimeProvider time)
    {
        this._time = time;
    }

    protected override ulong SchemaVersion => 124;

    protected override string Filename => "refreshGameServer.realm";
    
    protected override List<Type> SchemaTypes { get; } = new()
    {
        typeof(GameUser),
        typeof(GameLocation),
        typeof(UserPins),
        typeof(Token),
        typeof(GameLevel),
        typeof(GameSkillReward),
        typeof(GameComment),
        typeof(CommentRelation),
        typeof(FavouriteLevelRelation),
        typeof(QueueLevelRelation),
        typeof(FavouriteUserRelation),
        typeof(PlayLevelRelation),
        typeof(UniquePlayLevelRelation),
        typeof(RateLevelRelation),
        typeof(Event),
        typeof(GameSubmittedScore),
        typeof(GameAsset),
        typeof(GameNotification),
        typeof(GamePhoto),
        typeof(GamePhotoSubject),
        typeof(GameIpVerificationRequest),
        typeof(GameAnnouncement),
        typeof(QueuedRegistration),
        typeof(EmailVerificationCode),
        typeof(RequestStatistics),
        typeof(SequentialIdStorage),
        typeof(GameContest),
        //grief report items
        typeof(GameReport),
        typeof(InfoBubble),
        typeof(Marqee),
        typeof(Player),
        typeof(Rect),
        typeof(ScreenElements),
        typeof(ScreenRect),
        typeof(Slot),
        typeof(GameReview),
        typeof(DisallowedUser),
    };

    public override void Warmup()
    {
        using GameDatabaseContext context = this.GetContext();
        _ = context.GetTotalLevelCount();
    }

    protected override GameDatabaseContext CreateContext()
    {
        return new GameDatabaseContext(this._time);
    }

    protected override void Migrate(Migration migration, ulong oldVersion)
    {
        if (oldVersion != SchemaVersion)
            throw new InvalidOperationException($"The legacy database must be fully up to date before migration." +
                                                $"Expected version is {SchemaVersion}, but the database file was at {oldVersion}.");
    }
}