using Realms;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.Comments;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
using Bunkum.RealmDatabase;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Assets;
using Refresh.GameServer.Types.Comments.Relations;
using Refresh.GameServer.Types.Contests;
using Refresh.GameServer.Types.Levels.SkillRewards;
using Refresh.GameServer.Types.Notifications;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.UserData.Leaderboard;
using Refresh.GameServer.Types.Photos;

namespace Refresh.GameServer.Database;

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

    protected override ulong SchemaVersion => 133;

    protected override string Filename => "refreshGameServer.realm";
    
    protected override List<Type> SchemaTypes { get; } = new()
    {
        typeof(GameUser),
        typeof(UserPins),
        typeof(Token),
        typeof(GameLevel),
        typeof(GameSkillReward),
        typeof(GameProfileComment),
        typeof(GameLevelComment),
        typeof(ProfileCommentRelation),
        typeof(LevelCommentRelation),
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
        typeof(GameIpVerificationRequest),
        typeof(GameAnnouncement),
        typeof(QueuedRegistration),
        typeof(EmailVerificationCode),
        typeof(RequestStatistics),
        typeof(SequentialIdStorage),
        typeof(GameContest),
        typeof(AssetDependencyRelation),
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
        // Get the current unix timestamp for when we add timestamps to objects
        long timestampMilliseconds = this._time.TimestampMilliseconds;

        // DO NOT USE FOR NEW MIGRATIONS! LBP almost never actually uses seconds for timestamps.
        // This is from a mistake made early in development where this was not understood by me.
        // Unless you are certain second timestamps are used, use the millisecond timestamps set above.
        long timestampSeconds = this._time.TimestampSeconds;

        IQueryable<dynamic>? oldUsers = migration.OldRealm.DynamicApi.All("GameUser");
        IQueryable<GameUser>? newUsers = migration.NewRealm.All<GameUser>();

        for (int i = 0; i < newUsers.Count(); i++)
        {
            dynamic oldUser = oldUsers.ElementAt(i);
            GameUser newUser = newUsers.ElementAt(i);

            if (oldVersion < 3)
            {
                newUser.Description = "";
                newUser.LocationX = 0;
                newUser.LocationY = 0;
            }

            //In version 4, GameLocation went from TopLevel -> Embedded, and UserPins was added
            if (oldVersion < 4) newUser.Pins = new UserPins();

            // In version 12, users were given IconHashes
            if (oldVersion < 12) newUser.IconHash = "0";

            // In version 13, users were given PlanetsHashes
            if (oldVersion < 13) newUser.Lbp2PlanetsHash = "0";

            // In version 23, users were given bcrypt passwords
            if (oldVersion < 23) newUser.PasswordBcrypt = null;

            // In version 26, users were given join dates
            if (oldVersion < 26) newUser.JoinDate = DateTimeOffset.MinValue;

            // In version 40, we switched to Realm source generators which requires some values to be reset
            if (oldVersion < 40)
            {
                newUser.IconHash = oldUser.IconHash;
                newUser.Description = oldUser.Description;
                newUser.Lbp2PlanetsHash = oldUser.PlanetsHash;
            }
            
            // In version 67, users switched to dates to store their join date
            if (oldVersion < 67) newUser.JoinDate = DateTimeOffset.FromUnixTimeMilliseconds(oldUser.JoinDate);
            
            // In version 69 (nice), users were given last login dates. For now, we'll set that to now.
            if(oldVersion < 69 /*nice*/) newUser.LastLoginDate = DateTimeOffset.Now;
            
            // In version 72, users got settings for permissions regarding certain platforms.
            // To avoid breakage, we set them to true for existing users.
            if (oldVersion < 72)
            {
                newUser.PsnAuthenticationAllowed = true;
                newUser.RpcnAuthenticationAllowed = true;
            }
            
            // In version 81, we split out planet hashes for LBP2, LBP Vita, and LBP3
            // Since we've supported LBP2 as the primary game so far, set LBP2's hash to the old hash
            if (oldVersion < 81)
            {
                newUser.Lbp2PlanetsHash = oldUser.PlanetsHash;
                newUser.Lbp3PlanetsHash = oldUser.PlanetsHash; // Planets are forwards compatible
            }
            
            // Fix vita
            if (oldVersion < 82)
            {
                newUser.VitaPlanetsHash = "0";
            }

            // In version 100, we started enforcing lowercase email addresses
            if (oldVersion < 100) newUser.EmailAddress = oldUser.EmailAddress?.ToLowerInvariant();

            // Version was bumped here to delete invalid favourite level relations
            if (oldVersion < 101) migration.NewRealm.RemoveRange(migration.OldRealm.All<FavouriteLevelRelation>().Where(r => r.User.UserId == newUser.UserId && r.Level == null));

            // In version 102 we split the Vita icon hash from the PS3 icon hash
            if (oldVersion < 102) newUser.VitaIconHash = "0";
            
            //In version 117, we started tracking the amount of data the user has uploaded to the server
            if (oldVersion < 117)
            {
                newUser.FilesizeQuotaUsage = migration.NewRealm.All<GameAsset>()
                    .AsEnumerable()
                    .Where(a => a.OriginalUploader?.UserId == newUser.UserId)
                    .Sum(a => a.SizeInBytes);
            }
            
            // In version 129, we split locations from an embedded object out to two fields
            if (oldVersion < 129)
            {
                // cast required because apparently they're stored as longs???
                newUser.LocationX = (int)oldUser.Location.X;
                newUser.LocationY = (int)oldUser.Location.Y;
            }
        }

        IQueryable<dynamic>? oldLevels = migration.OldRealm.DynamicApi.All("GameLevel");
        IQueryable<GameLevel>? newLevels = migration.NewRealm.All<GameLevel>();

        for (int i = 0; i < newLevels.Count(); i++)
        {
            dynamic oldLevel = oldLevels.ElementAt(i);
            GameLevel newLevel = newLevels.ElementAt(i);

            // In version 10, GameLevels switched to int-based ids.
            if (oldVersion < 10)
            {
                newLevel.LevelId = i + 1;
            }

            // In version 11, timestamps were added to levels.
            if (oldVersion < 11)
            {
                // Since we dont have a reference point for when the level was actually uploaded, default to now
                newLevel.PublishDate = timestampSeconds;
                newLevel.UpdateDate = timestampSeconds;
            }

            // In version 14, level timestamps were fixed
            if (oldVersion < 14)
            {
                newLevel.PublishDate = oldLevel.PublishDate * 1000;
                newLevel.UpdateDate = oldLevel.UpdateDate * 1000;
            }
            
            // In version 40, we switched to Realm source generators which requires some values to be reset
            if (oldVersion < 40)
            {
                newLevel.Title = oldLevel.Title;
                newLevel.IconHash = oldLevel.IconHash;
                newLevel.Description = oldLevel.Description;
                newLevel.RootResource = oldLevel.RootResource;
            }

            // In version 57, we implemented minimum and maximum players which are 1 and 4 by default
            if (oldVersion < 57)
            {
                newLevel.MinPlayers = 1;
                newLevel.MaxPlayers = 4;
            }
            
            // In version 79, we started tracking the version in which the level was uploaded from
            // Set all levels to LBP2 by default, since that's the version we've supported up until now.
            if (oldVersion < 79)
            {
                newLevel._GameVersion = (int)TokenGame.LittleBigPlanet2;
            }

            // In version 92, we started storing both user and story levels.
            // Set all existing levels to user levels, since that's what has existed up until now.
            if (oldVersion < 92)
            {
                newLevel._Source = (int)GameLevelSource.User;
            }
            
            // In version 129, we split locations from an embedded object out to two fields
            if (oldVersion < 129)
            {
                newLevel.LocationX = (int)oldLevel.Location.X;
                newLevel.LocationY = (int)oldLevel.Location.Y;
            }
        }

        // In version 22, tokens added expiry and types so just wipe them all
        if (oldVersion < 22) migration.NewRealm.RemoveAll<Token>();
                
        // In version 35, tokens added platforms and games
        if (oldVersion < 35) migration.NewRealm.RemoveAll<Token>();

        // IQueryable<dynamic>? oldEvents = migration.OldRealm.DynamicApi.All("Event");
        IQueryable<Event>? newEvents = migration.NewRealm.All<Event>();

        List<Event> eventsToNuke = new();
        for (int i = 0; i < newEvents.Count(); i++)
        {
            // dynamic oldEvent = oldEvents.ElementAt(i);
            Event newEvent = newEvents.ElementAt(i);

            // In version 30, events were given timestamps
            if (oldVersion < 30) newEvent.Timestamp = timestampSeconds;

            // Fixes events with broken timestamps
            if (oldVersion < 32 && newEvent.Timestamp == 0) newEvent.Timestamp = timestampSeconds;

            // Converts events to use millisecond timestamps
            if (oldVersion < 33 && newEvent.Timestamp < 1000000000000) newEvent.Timestamp *= 1000;

            // fixup for dumb bad code not clearing score events when levels are deleted
            if (oldVersion < 96 && newEvent.StoredDataType == EventDataType.Score)
            {
                GameSubmittedScore? score = migration.NewRealm.All<GameSubmittedScore>().FirstOrDefault(s => s.ScoreId == newEvent.StoredObjectId);
                if(score == null) eventsToNuke.Add(newEvent);
            }
            
            // In version 131 we removed the LevelPlay event
            if (oldVersion < 131 && newEvent.EventType == EventType.LevelPlay)
            {
                eventsToNuke.Add(newEvent);
            }
        }
        
        // realm won't let you use an IEnumerable in RemoveRange. too bad!
        foreach (Event eventToNuke in eventsToNuke)
        {
            migration.NewRealm.Remove(eventToNuke);
        }
        
        // In version 126, we started tracking token IP, there's no way for us to acquire this after the fact, so lets just clear all the tokens
        if (oldVersion < 126) 
            migration.NewRealm.RemoveAll<Token>();
        
        // IQueryable<dynamic>? oldTokens = migration.OldRealm.DynamicApi.All("Token");
        IQueryable<Token>? newTokens = migration.NewRealm.All<Token>();

        for (int i = 0; i < newTokens.Count(); i++)
        {
            // dynamic oldToken = oldTokens.ElementAt(i);
            Token newToken = newTokens.ElementAt(i);

            if (oldVersion < 36) newToken.LoginDate = DateTimeOffset.FromUnixTimeMilliseconds(timestampMilliseconds);
        }
        
        IQueryable<dynamic>? oldComments = migration.OldRealm.DynamicApi.All("GameComment");
        IQueryable<dynamic>? newComments = migration.NewRealm.DynamicApi.All("GameComment");

        for (int i = 0; i < newComments.Count(); i++)
        {
            dynamic oldComment = oldComments.ElementAt(i);
            dynamic newComment = newComments.ElementAt(i);

            // In version 40, we switched to Realm source generators, which requires some values to be reset
            if (oldVersion < 40)
            {
                newComment.Content = oldComment.Content;
            }
        }
        
        IQueryable<dynamic>? oldPhotos = migration.OldRealm.DynamicApi.All("GamePhoto");
        IQueryable<GamePhoto>? newPhotos = migration.NewRealm.All<GamePhoto>();

        for (int i = 0; i < oldPhotos.Count(); i++)
        {
            dynamic oldPhoto = oldPhotos.ElementAt(i);
            GamePhoto newPhoto = newPhotos.ElementAt(i);

            // In version 52, the timestamp on photos were corrected
            if (oldVersion < 52)
            {
                newPhoto.TakenAt = DateTimeOffset.FromUnixTimeSeconds(oldPhoto.TakenAt.ToUnixTimeMilliseconds());
            }

            if (oldVersion < 110)
            {
                newPhoto.LargeAsset = migration.NewRealm.Find<GameAsset>(oldPhoto.LargeHash.StartsWith("psp/") ? oldPhoto.LargeHash.Substring(4) : oldPhoto.LargeHash);
                newPhoto.MediumAsset = migration.NewRealm.Find<GameAsset>(oldPhoto.MediumHash.StartsWith("psp/") ? oldPhoto.MediumHash.Substring(4) : oldPhoto.MediumHash);
                newPhoto.SmallAsset = migration.NewRealm.Find<GameAsset>(oldPhoto.SmallHash.StartsWith("psp/") ? oldPhoto.SmallHash.Substring(4) : oldPhoto.SmallHash);
            }

            // In version 133, we removed GamePhotoSubject from the schema entirely, and instead we put 4 'unrolled' sets of fields in the GamePhoto.
            // This makes things chaotic code-wise, but is significantly more compatible with Postgres.
            if (oldVersion < 133)
            {
                List<GamePhotoSubject> oldSubjects = new(oldPhoto.Subjects.Count);
                foreach(dynamic subject in oldPhoto.Subjects)
                {
                    List<float> bounds = new(4);
                    foreach (float bound in subject.Bounds) bounds.Add(bound);

                    GameUser? user = subject.User != null ? migration.NewRealm.Find<GameUser>(subject.User.UserId) : null;
                    oldSubjects.Add(new GamePhotoSubject(user, subject.DisplayName, bounds));
                }

                Console.WriteLine(JsonConvert.SerializeObject(oldSubjects));
                
                newPhoto.Subjects = oldSubjects;
            }
        }
        
        IQueryable<dynamic>? oldAssets = migration.OldRealm.DynamicApi.All("GameAsset");
        IQueryable<GameAsset>? newAssets = migration.NewRealm.All<GameAsset>();

        for (int i = 0; i < newAssets.Count(); i++)
        {
            dynamic oldAsset = oldAssets.ElementAt(i);
            GameAsset newAsset = newAssets.ElementAt(i);
            
            if (oldVersion < 88)
            {
                // We don't have any more advanced heuristics here,
                // but TGA files are the only asset currently affected by the tracking of `IsPSP`,
                // and PSP is the only game to upload TGA files.
                newAsset.IsPSP = newAsset.AssetType == GameAssetType.Tga;
            }

            // In version 128 assets were moved from a list on the asset to a separate "relations" table
            if (oldVersion < 128)
            {
                IList<string> dependencies = oldAsset.Dependencies;
                
                foreach (string dependency in dependencies)
                {
                    migration.NewRealm.Add(new AssetDependencyRelation
                    {
                        Dependent = oldAsset.AssetHash,
                        Dependency = dependency,
                    });
                }
            }
        }
        
        // Remove all scores with a null level, as in version 92 we started tracking story leaderboards differently
        if (oldVersion < 92) migration.NewRealm.RemoveRange(migration.NewRealm.All<GameSubmittedScore>().Where(s => s.Level == null));
        
        IQueryable<dynamic>? oldScores = migration.OldRealm.DynamicApi.All("GameSubmittedScore");
        IQueryable<GameSubmittedScore>? newScores = migration.NewRealm.All<GameSubmittedScore>();

        for (int i = 0; i < newScores.Count(); i++)
        {
            dynamic oldScore = oldScores.ElementAt(i);
            GameSubmittedScore newScore = newScores.ElementAt(i);

            if (oldVersion < 92)
            {
                newScore.Game = newScore.Level.GameVersion;
            }

            // In version 104 we started tracking the platform
            if (oldVersion < 104)
            {
                // Determine the most reasonable platform for the score's game
                TokenPlatform platform = newScore.Game switch
                {
                    TokenGame.LittleBigPlanet1 => TokenPlatform.PS3,
                    TokenGame.LittleBigPlanet2 => TokenPlatform.PS3,
                    TokenGame.LittleBigPlanet3 => TokenPlatform.PS3,
                    TokenGame.LittleBigPlanetVita => TokenPlatform.Vita,
                    TokenGame.LittleBigPlanetPSP => TokenPlatform.PSP,
                    TokenGame.BetaBuild => TokenPlatform.RPCS3,
                    TokenGame.Website => throw new InvalidOperationException($"what? score id {newScore.ScoreId} by {newScore.Players[0].Username} is fucked"),
                    _ => throw new ArgumentOutOfRangeException(),
                };

                newScore.Platform = platform;
            }
        }
        
        IQueryable<dynamic>? oldPlayLevelRelations = migration.OldRealm.DynamicApi.All("PlayLevelRelation");
        IQueryable<PlayLevelRelation>? newPlayLevelRelations = migration.NewRealm.All<PlayLevelRelation>();

        for (int i = 0; i < newPlayLevelRelations.Count(); i++)
        {
            dynamic oldPlayLevelRelation = oldPlayLevelRelations.ElementAt(i);
            PlayLevelRelation newPlayLevelRelation = newPlayLevelRelations.ElementAt(i);

            //In version 93, we added a count to PlayLevelRelation
            if (oldVersion < 93)
            {
                newPlayLevelRelation.Count = 1;
            }
        }
    }
}