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
using Refresh.GameServer.Types.Playlists;

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

    protected override ulong SchemaVersion => 162;

    protected override string Filename => "refreshGameServer.realm";
    
    protected override List<Type> SchemaTypes { get; } =
    [
        typeof(RequestStatistics),
        typeof(SequentialIdStorage),

        typeof(Event),
        typeof(GameAnnouncement),
        typeof(GameContest),
        typeof(GamePhoto),

        // levels
        typeof(GameLevel),
        typeof(GameSkillReward),
        typeof(TagLevelRelation),
        typeof(GameLevelComment),
        typeof(LevelCommentRelation),
        typeof(RateLevelRelation),
        typeof(FavouriteLevelRelation),
        typeof(PlayLevelRelation),
        typeof(UniquePlayLevelRelation),
        typeof(QueueLevelRelation),
        typeof(GameSubmittedScore),

        // reviews
        typeof(GameReview),
        typeof(RateReviewRelation),

        // users
        typeof(GameUser),
        typeof(Token),
        typeof(UserPins),
        typeof(GameProfileComment),
        typeof(FavouriteUserRelation),
        typeof(DisallowedUser),
        typeof(GameNotification),
        typeof(ProfileCommentRelation),
        typeof(EmailVerificationCode),
        typeof(QueuedRegistration),
        typeof(GameIpVerificationRequest),
        typeof(GameUserVerifiedIpRelation),
        typeof(LevelUploads),

        // assets
        typeof(GameAsset),
        typeof(AssetDependencyRelation),
        
        // playlists
        typeof(GamePlaylist),
        typeof(LevelPlaylistRelation),
        typeof(SubPlaylistRelation),
        typeof(FavouritePlaylistRelation),
    ];

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
        IQueryable<dynamic>? oldUsers = migration.OldRealm.DynamicApi.All("GameUser");
        IQueryable<GameUser>? newUsers = migration.NewRealm.All<GameUser>();
        if (oldVersion < 161)
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
                if (oldVersion < 69 /*nice*/) newUser.LastLoginDate = this._time.Now;

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
                if (oldVersion < 101)
                    migration.NewRealm.RemoveRange(migration.OldRealm.All<FavouriteLevelRelation>()
                        .Where(r => r.User.UserId == newUser.UserId && r.Level == null));

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

                // In version 134, we split GameComments into multiple tables.
                // This migration creates GameProfileComments
                if (oldVersion < 134)
                {
                    foreach (dynamic comment in oldUser.ProfileComments)
                    {
                        GameUser? author = comment.Author != null
                            ? migration.NewRealm.Find<GameUser>(comment.Author.UserId)
                            : null;
                        if (author == null)
                        {
                            Console.WriteLine(
                                $"Skipping migration for profile comment id {comment.SequentialId} due to missing author");
                            continue;
                        }

                        migration.NewRealm.Add(new GameProfileComment
                        {
                            SequentialId = (int)comment.SequentialId,
                            Author = author,
                            Profile = newUser,
                            Content = comment.Content,
                            Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(comment.Timestamp),
                        });
                    }
                }

                if (oldVersion < 145)
                    newUser.ShowModdedContent = true;

                // In version 161, we allowed users to set multiple verified IPs
                if (oldVersion < 161 && newUser.AllowIpAuthentication && oldUser.CurrentVerifiedIp != null)
                {
                    migration.NewRealm.Add(new GameUserVerifiedIpRelation
                    {
                        User = newUser,
                        IpAddress = oldUser.CurrentVerifiedIp, 
                        VerifiedAt = this._time.Now,
                    });
                }
            }

        IQueryable<dynamic>? oldLevels = migration.OldRealm.DynamicApi.All("GameLevel");
        IQueryable<GameLevel>? newLevels = migration.NewRealm.All<GameLevel>();

        if (oldVersion < 149)
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
                    newLevel.PublishDate = this._time.Now;
                    newLevel.UpdateDate = this._time.Now;
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

                // In version 129, we split locations from an embedded object out to two fields
                if (oldVersion < 129)
                {
                    newLevel.LocationX = (int)oldLevel.Location.X;
                    newLevel.LocationY = (int)oldLevel.Location.Y;
                }

                // In version 134, we split GameComments into multiple tables.
                // This migration creates GameLevelComments
                if (oldVersion < 134)
                {
                    foreach (dynamic comment in oldLevel.LevelComments)
                    {
                        GameUser? author = comment.Author != null
                            ? migration.NewRealm.Find<GameUser>(comment.Author.UserId)
                            : null;
                        if (author == null)
                        {
                            Console.WriteLine(
                                $"Skipping migration for level comment id {comment.SequentialId} due to missing author");
                            continue;
                        }

                        migration.NewRealm.Add(new GameLevelComment
                        {
                            SequentialId = (int)comment.SequentialId,
                            Author = author,
                            Level = newLevel,
                            Content = comment.Content,
                            Timestamp = comment.Timestamp,
                        });
                    }
                }

                // In version 137 we started storing the date at which levels were picked instead of just that they are picked.
                // Since this is new information, let's set this to the date of the last update. 
                if (oldVersion < 137)
                {
                    if (oldLevel.TeamPicked)
                        newLevel.DateTeamPicked = DateTimeOffset.FromUnixTimeMilliseconds(oldLevel.UpdateDate);
                    else
                        newLevel.DateTeamPicked = null;
                }

                // In version 138 we added support for Adventures in LBP3. Set their status to false by default.
                if (oldVersion < 138)
                {
                    newLevel.IsAdventure = false;
                }

                // In version 140, we migrated from unix milliseconds timestamps to DateTimeOffsets
                if (oldVersion < 140)
                {
                    newLevel.PublishDate = DateTimeOffset.FromUnixTimeMilliseconds(oldLevel.PublishDate);
                    newLevel.UpdateDate = DateTimeOffset.FromUnixTimeMilliseconds(oldLevel.UpdateDate);
                }

                // Version 148 is when `Modded` was added, and in 149 we renamed `Modded` to `IsModded`
                if (oldVersion >= 148 && oldVersion < 149)
                {
                    newLevel.IsModded = oldLevel.Modded;
                }
            }

        // In version 22, tokens added expiry and types so just wipe them all
        if (oldVersion < 22) migration.NewRealm.RemoveAll<Token>();
                
        // In version 35, tokens added platforms and games
        if (oldVersion < 35) migration.NewRealm.RemoveAll<Token>();

        IQueryable<dynamic>? oldEvents = migration.OldRealm.DynamicApi.All("Event");
        IQueryable<Event>? newEvents = migration.NewRealm.All<Event>();

        List<Event> eventsToNuke = new();
        if (oldVersion < 140)
            for (int i = 0; i < newEvents.Count(); i++)
            {
                dynamic oldEvent = oldEvents.ElementAt(i);
                Event newEvent = newEvents.ElementAt(i);

                // In version 30, events were given timestamps
                // Version 32 fixes events with broken timestamps
                if (oldVersion < 30 || oldVersion < 32 && newEvent.Timestamp.ToUnixTimeMilliseconds() == 0)
                    newEvent.Timestamp = this._time.Now;

                // Converts events to use millisecond timestamps
                if (oldVersion < 33 && newEvent.Timestamp.ToUnixTimeMilliseconds() < 1000000000000)
                    newEvent.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(oldEvent.Timestamp * 1000);

                // fixup for dumb bad code not clearing score events when levels are deleted
                if (oldVersion < 96 && newEvent.StoredDataType == EventDataType.Score)
                {
                    GameSubmittedScore? score = migration.NewRealm.All<GameSubmittedScore>()
                        .FirstOrDefault(s => s.ScoreId == newEvent.StoredObjectId);
                    if (score == null) eventsToNuke.Add(newEvent);
                }

                // In version 131, we removed the LevelPlay event
                if (oldVersion < 131 && newEvent.EventType == EventType.LevelPlay)
                    eventsToNuke.Add(newEvent);

                // In version 140, we migrated from unix milliseconds timestamps to DateTimeOffsets
                if (oldVersion < 140)
                    newEvent.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(oldEvent.Timestamp);
            }

        // realm won't let you use an IEnumerable in RemoveRange. too bad!
        foreach (Event eventToNuke in eventsToNuke)
        {
            migration.NewRealm.Remove(eventToNuke);
        }
        
        // In version 126, we started tracking token IP; there's no way for us to acquire this after the fact, so let's just clear all the tokens
        if (oldVersion < 126) 
            migration.NewRealm.RemoveAll<Token>();
        
        // IQueryable<dynamic>? oldTokens = migration.OldRealm.DynamicApi.All("Token");
        IQueryable<Token>? newTokens = migration.NewRealm.All<Token>();

        if (oldVersion < 36)
            for (int i = 0; i < newTokens.Count(); i++)
            {
                // dynamic oldToken = oldTokens.ElementAt(i);
                Token newToken = newTokens.ElementAt(i);

                if (oldVersion < 36)
                    newToken.LoginDate = this._time.Now;
            }

        IQueryable<dynamic>? oldPhotos = migration.OldRealm.DynamicApi.All("GamePhoto");
        IQueryable<GamePhoto>? newPhotos = migration.NewRealm.All<GamePhoto>();

        if (oldVersion < 133)
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
                    newPhoto.LargeAsset = migration.NewRealm.Find<GameAsset>(oldPhoto.LargeHash.StartsWith("psp/")
                        ? oldPhoto.LargeHash.Substring(4)
                        : oldPhoto.LargeHash);
                    newPhoto.MediumAsset = migration.NewRealm.Find<GameAsset>(oldPhoto.MediumHash.StartsWith("psp/")
                        ? oldPhoto.MediumHash.Substring(4)
                        : oldPhoto.MediumHash);
                    newPhoto.SmallAsset = migration.NewRealm.Find<GameAsset>(oldPhoto.SmallHash.StartsWith("psp/")
                        ? oldPhoto.SmallHash.Substring(4)
                        : oldPhoto.SmallHash);
                }

                // In version 133, we removed GamePhotoSubject from the schema entirely, and instead we put 4 'unrolled' sets of fields in the GamePhoto.
                // This makes things chaotic code-wise, but is significantly more compatible with Postgres.
                if (oldVersion < 133)
                {
                    List<GamePhotoSubject> oldSubjects = new(oldPhoto.Subjects.Count);
                    foreach (dynamic subject in oldPhoto.Subjects)
                    {
                        List<float> bounds = new(4);
                        foreach (float bound in subject.Bounds) bounds.Add(bound);

                        GameUser? user = subject.User != null
                            ? migration.NewRealm.Find<GameUser>(subject.User.UserId)
                            : null;
                        oldSubjects.Add(new GamePhotoSubject(user, subject.DisplayName, bounds));
                    }

                    newPhoto.Subjects = oldSubjects;
                }
            }

        IQueryable<dynamic>? oldAssets = migration.OldRealm.DynamicApi.All("GameAsset");
        IQueryable<GameAsset>? newAssets = migration.NewRealm.All<GameAsset>();

        if (oldVersion < 136)
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

                // In version 136, we started tracking asset types differently, so we need to convert to the new system
                if (oldVersion < 136)
                {
                    newAsset.AssetType = (int)oldAsset._AssetType switch
                    {
                        -1 => GameAssetType.Unknown,
                        0 => GameAssetType.Level,
                        1 => GameAssetType.Plan,
                        2 => GameAssetType.Texture,
                        3 => GameAssetType.Jpeg,
                        4 => GameAssetType.Png,
                        5 => GameAssetType.GfxMaterial,
                        6 => GameAssetType.Mesh,
                        7 => GameAssetType.GameDataTexture,
                        8 => GameAssetType.Palette,
                        9 => GameAssetType.Script,
                        10 => GameAssetType.ThingRecording,
                        11 => GameAssetType.VoiceRecording,
                        12 => GameAssetType.Painting,
                        13 => GameAssetType.Tga,
                        14 => GameAssetType.SyncedProfile,
                        15 => GameAssetType.Mip,
                        16 => GameAssetType.GriefSongState,
                        17 => GameAssetType.Material,
                        18 => GameAssetType.SoftPhysicsSettings,
                        19 => GameAssetType.Bevel,
                        20 => GameAssetType.StreamingLevelChunk,
                        21 => GameAssetType.Animation,
                        _ => throw new Exception("Invalid asset type " + oldAsset._AssetType),
                    };
                    newAsset.AssetFormat = (int)oldAsset._AssetType switch
                    {
                        -1 => GameAssetFormat.Unknown,
                        0 => GameAssetFormat.Binary,
                        1 => GameAssetFormat.Binary,
                        2 => GameAssetFormat.CompressedTexture,
                        3 => GameAssetFormat.Unknown,
                        4 => GameAssetFormat.Unknown,
                        5 => GameAssetFormat.Binary,
                        6 => GameAssetFormat.Binary,
                        7 => GameAssetFormat.CompressedTexture,
                        8 => GameAssetFormat.Binary,
                        9 => GameAssetFormat.Binary,
                        10 => GameAssetFormat.Binary,
                        11 => GameAssetFormat.Binary,
                        12 => GameAssetFormat.Binary,
                        13 => GameAssetFormat.Unknown,
                        14 => GameAssetFormat.Binary,
                        15 => GameAssetFormat.Unknown,
                        16 => GameAssetFormat.Unknown,
                        17 => GameAssetFormat.Binary,
                        18 => GameAssetFormat.Text,
                        19 => GameAssetFormat.Binary,
                        20 => GameAssetFormat.Binary,
                        21 => GameAssetFormat.Binary,
                        _ => throw new Exception("Invalid asset type " + oldAsset._AssetType),
                    };
                }
            }

        // Remove all scores with a null level, as in version 92 we started tracking story leaderboards differently
        if (oldVersion < 92)
            migration.NewRealm.RemoveRange(migration.NewRealm.All<GameSubmittedScore>().Where(s => s.Level == null));

        // fuck realm.
        if (oldVersion < 152)
            foreach (GameSubmittedScore score in migration.NewRealm.All<GameSubmittedScore>().AsEnumerable()
                         .Where(s => !s.Players.Any()).ToList())
                migration.NewRealm.Remove(score);

        IQueryable<dynamic>? oldScores = migration.OldRealm.DynamicApi.All("GameSubmittedScore");
        IQueryable<GameSubmittedScore>? newScores = migration.NewRealm.All<GameSubmittedScore>();

        if (oldVersion < 104)
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
                        TokenGame.Website => throw new InvalidOperationException(
                            $"what? score id {newScore.ScoreId} by {newScore.Players[0].Username} is fucked"),
                        _ => throw new ArgumentOutOfRangeException(),
                    };

                    newScore.Platform = platform;
                }
            }

        IQueryable<dynamic>? oldPlayLevelRelations = migration.OldRealm.DynamicApi.All("PlayLevelRelation");
        IQueryable<PlayLevelRelation>? newPlayLevelRelations = migration.NewRealm.All<PlayLevelRelation>();

        if (oldVersion < 140)
            for (int i = 0; i < newPlayLevelRelations.Count(); i++)
            {
                dynamic oldPlayLevelRelation = oldPlayLevelRelations.ElementAt(i);
                PlayLevelRelation newPlayLevelRelation = newPlayLevelRelations.ElementAt(i);

                //In version 93, we added a count to PlayLevelRelation
                if (oldVersion < 93)
                {
                    newPlayLevelRelation.Count = 1;
                }

                // In version 140, we migrated from unix milliseconds timestamps to DateTimeOffsets
                if (oldVersion < 140)
                    newPlayLevelRelation.Timestamp =
                        DateTimeOffset.FromUnixTimeMilliseconds(oldPlayLevelRelation.Timestamp);
            }

        // We weren't deleting reviews when a level was deleted. Version 139 fixes this.
        if (oldVersion < 139)
            migration.NewRealm.RemoveRange(migration.NewRealm.All<GameReview>().Where(r => r.Level == null));
        
        IQueryable<dynamic>? oldLevelComments = migration.OldRealm.DynamicApi.All("GameLevelComment");
        IQueryable<GameLevelComment>? newLevelComments = migration.NewRealm.All<GameLevelComment>();

        if (oldVersion < 140)
            for (int i = 0; i < newLevelComments.Count(); i++)
            {
                dynamic oldLevelComment = oldLevelComments.ElementAt(i);
                GameLevelComment newLevelComment = newLevelComments.ElementAt(i);

                // In version 140, we migrated from unix milliseconds timestamps to DateTimeOffsets
                if (oldVersion < 140)
                    newLevelComment.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(oldLevelComment.Timestamp);
            }

        IQueryable<dynamic>? oldProfileComments = migration.OldRealm.DynamicApi.All("GameProfileComment");
        IQueryable<GameProfileComment>? newProfileComments = migration.NewRealm.All<GameProfileComment>();

        if (oldVersion < 140)
            for (int i = 0; i < newProfileComments.Count(); i++)
            {
                dynamic oldProfileComment = oldProfileComments.ElementAt(i);
                GameProfileComment newProfileComment = newProfileComments.ElementAt(i);

                // In version 140, we migrated from unix milliseconds timestamps to DateTimeOffsets
                if (oldVersion < 140)
                    newProfileComment.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(oldProfileComment.Timestamp);
            }

        IQueryable<dynamic>? oldUniquePlayLevelRelations = migration.OldRealm.DynamicApi.All("UniquePlayLevelRelation");
        IQueryable<UniquePlayLevelRelation>? newUniquePlayLevelRelations = migration.NewRealm.All<UniquePlayLevelRelation>();

        if (oldVersion < 140)
            for (int i = 0; i < newUniquePlayLevelRelations.Count(); i++)
            {
                dynamic oldUniquePlayLevelRelation = oldUniquePlayLevelRelations.ElementAt(i);
                UniquePlayLevelRelation newUniquePlayLevelRelation = newUniquePlayLevelRelations.ElementAt(i);

                // In version 140, we migrated from unix milliseconds timestamps to DateTimeOffsets
                if (oldVersion < 140)
                    newUniquePlayLevelRelation.Timestamp =
                        DateTimeOffset.FromUnixTimeMilliseconds(oldUniquePlayLevelRelation.Timestamp);
            }
        
        // IQueryable<dynamic>? oldGamePlaylists = migration.OldRealm.DynamicApi.All("GamePlaylist");
        // IQueryable<GamePlaylist>? newGamePlaylists = migration.NewRealm.All<GamePlaylist>();
        
        // if (oldVersion < 155)
        //     for (int i = 0; i < newGamePlaylists.Count(); i++)
        //     {
        //         dynamic oldGamePlaylist = oldGamePlaylists.ElementAt(i);
        //         GamePlaylist newGamePlaylist = newGamePlaylists.ElementAt(i);
        //     }
            
        // We weren't deleting level playlist relations when a level was deleted. Version 160 fixes this.
        if (oldVersion < 160)
            migration.NewRealm.RemoveRange(migration.NewRealm.All<LevelPlaylistRelation>().Where(r => r.Level == null));
        
        // Version 162 added indices for LevelPlaylistRelations for custom playlist level ordering
        IQueryable<dynamic>? oldLevelPlaylistRelations = migration.OldRealm.DynamicApi.All("LevelPlaylistRelation");
        IQueryable<LevelPlaylistRelation>? newLevelPlaylistRelations = migration.NewRealm.All<LevelPlaylistRelation>();
        if (oldVersion < 162)
            for (int i = 0; i < newLevelPlaylistRelations.Count(); i++)
            {
                dynamic oldLevelPlaylistRelation = oldLevelPlaylistRelations.ElementAt(i);
                LevelPlaylistRelation newLevelPlaylistRelation = newLevelPlaylistRelations.ElementAt(i);

                if (oldVersion < 162)
                {
                    newLevelPlaylistRelation.Index = 0;
                }
            }

        // IQueryable<dynamic>? oldSubPlaylistRelations = migration.OldRealm.DynamicApi.All("SubPlaylistRelation");
        // IQueryable<SubPlaylistRelation>? newSubPlaylistRelations = migration.NewRealm.All<SubPlaylistRelation>();
        // if (oldVersion < 155)
        //     for (int i = 0; i < newSubPlaylistRelations.Count(); i++)
        //     {
        //         dynamic oldSubPlaylistRelation = oldSubPlaylistRelations.ElementAt(i);
        //         SubPlaylistRelation newSubPlaylistRelation = newSubPlaylistRelations.ElementAt(i);
        //     }
    }
}