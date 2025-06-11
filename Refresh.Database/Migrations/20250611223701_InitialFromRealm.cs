using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250611223701_InitialFromRealm")]
    public partial class InitialFromRealm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetDependencyRelations",
                columns: table => new
                {
                    Dependent = table.Column<string>(type: "text", nullable: false),
                    Dependency = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetDependencyRelations", x => new { x.Dependent, x.Dependency });
                });

            migrationBuilder.CreateTable(
                name: "DisallowedUsers",
                columns: table => new
                {
                    Username = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisallowedUsers", x => x.Username);
                });

            migrationBuilder.CreateTable(
                name: "GameAnnouncements",
                columns: table => new
                {
                    AnnouncementId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameAnnouncements", x => x.AnnouncementId);
                });

            migrationBuilder.CreateTable(
                name: "QueuedRegistrations",
                columns: table => new
                {
                    RegistrationId = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: true),
                    EmailAddress = table.Column<string>(type: "text", nullable: true),
                    PasswordBcrypt = table.Column<string>(type: "text", nullable: true),
                    ExpiryDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueuedRegistrations", x => x.RegistrationId);
                });

            migrationBuilder.CreateTable(
                name: "RequestStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TotalRequests = table.Column<long>(type: "bigint", nullable: false),
                    ApiRequests = table.Column<long>(type: "bigint", nullable: false),
                    GameRequests = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestStatistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SequentialIdStorage",
                columns: table => new
                {
                    TypeName = table.Column<string>(type: "text", nullable: false),
                    SequentialId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SequentialIdStorage", x => x.TypeName);
                });

            migrationBuilder.CreateTable(
                name: "EmailVerificationCodes",
                columns: table => new
                {
                    Code = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ExpiryDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerificationCodes", x => new { x.UserId, x.Code });
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventId = table.Column<string>(type: "text", nullable: false),
                    UserId1 = table.Column<string>(type: "text", nullable: true),
                    IsPrivate = table.Column<bool>(type: "boolean", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StoredSequentialId = table.Column<int>(type: "integer", nullable: true),
                    StoredObjectId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "FavouriteLevelRelations",
                columns: table => new
                {
                    LevelId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavouriteLevelRelations", x => new { x.LevelId, x.UserId });
                });

            migrationBuilder.CreateTable(
                name: "FavouritePlaylistRelations",
                columns: table => new
                {
                    PlaylistId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavouritePlaylistRelations", x => new { x.UserId, x.PlaylistId });
                });

            migrationBuilder.CreateTable(
                name: "FavouriteUserRelations",
                columns: table => new
                {
                    UserToFavouriteId = table.Column<string>(type: "text", nullable: false),
                    UserFavouritingId = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavouriteUserRelations", x => new { x.UserToFavouriteId, x.UserFavouritingId });
                });

            migrationBuilder.CreateTable(
                name: "GameAssets",
                columns: table => new
                {
                    AssetHash = table.Column<string>(type: "text", nullable: false),
                    OriginalUploaderUserId = table.Column<string>(type: "text", nullable: true),
                    UploadDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsPSP = table.Column<bool>(type: "boolean", nullable: false),
                    SizeInBytes = table.Column<int>(type: "integer", nullable: false),
                    AsMainlineIconHash = table.Column<string>(type: "text", nullable: true),
                    AsMipIconHash = table.Column<string>(type: "text", nullable: true),
                    AsMainlinePhotoHash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameAssets", x => x.AssetHash);
                });

            migrationBuilder.CreateTable(
                name: "GameChallenges",
                columns: table => new
                {
                    ChallengeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PublisherUserId = table.Column<string>(type: "text", nullable: true),
                    LevelId = table.Column<int>(type: "integer", nullable: true),
                    StartCheckpointUid = table.Column<int>(type: "integer", nullable: false),
                    FinishCheckpointUid = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<byte>(type: "smallint", nullable: false),
                    _Type = table.Column<byte>(type: "smallint", nullable: false),
                    PublishDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdateDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpirationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SequentialId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameChallenges", x => x.ChallengeId);
                });

            migrationBuilder.CreateTable(
                name: "GameChallengeScores",
                columns: table => new
                {
                    ScoreId = table.Column<string>(type: "text", nullable: false),
                    ChallengeId = table.Column<int>(type: "integer", nullable: true),
                    PublisherUserId = table.Column<string>(type: "text", nullable: true),
                    Score = table.Column<long>(type: "bigint", nullable: false),
                    GhostHash = table.Column<string>(type: "text", nullable: true),
                    Time = table.Column<long>(type: "bigint", nullable: false),
                    PublishDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameChallengeScores", x => x.ScoreId);
                    table.ForeignKey(
                        name: "FK_GameChallengeScores_GameChallenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "GameChallenges",
                        principalColumn: "ChallengeId");
                });

            migrationBuilder.CreateTable(
                name: "GameContests",
                columns: table => new
                {
                    ContestId = table.Column<string>(type: "text", nullable: false),
                    OrganizerUserId = table.Column<string>(type: "text", nullable: true),
                    CreationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ContestTag = table.Column<string>(type: "text", nullable: true),
                    BannerUrl = table.Column<string>(type: "text", nullable: true),
                    ContestTitle = table.Column<string>(type: "text", nullable: true),
                    ContestSummary = table.Column<string>(type: "text", nullable: true),
                    ContestDetails = table.Column<string>(type: "text", nullable: true),
                    ContestTheme = table.Column<string>(type: "text", nullable: true),
                    AllowedGames = table.Column<int[]>(type: "integer[]", nullable: true),
                    TemplateLevelLevelId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameContests", x => x.ContestId);
                });

            migrationBuilder.CreateTable(
                name: "GameIpVerificationRequests",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameIpVerificationRequests", x => new { x.UserId, x.IpAddress });
                });

            migrationBuilder.CreateTable(
                name: "GameLevelComments",
                columns: table => new
                {
                    SequentialId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuthorUserId = table.Column<string>(type: "text", nullable: true),
                    LevelId = table.Column<int>(type: "integer", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameLevelComments", x => x.SequentialId);
                });

            migrationBuilder.CreateTable(
                name: "GameLevels",
                columns: table => new
                {
                    LevelId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsAdventure = table.Column<bool>(type: "boolean", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    IconHash = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    LocationX = table.Column<int>(type: "integer", nullable: false),
                    LocationY = table.Column<int>(type: "integer", nullable: false),
                    RootResource = table.Column<string>(type: "text", nullable: false),
                    PublishDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MinPlayers = table.Column<int>(type: "integer", nullable: false),
                    MaxPlayers = table.Column<int>(type: "integer", nullable: false),
                    EnforceMinMaxPlayers = table.Column<bool>(type: "boolean", nullable: false),
                    SameScreenGame = table.Column<bool>(type: "boolean", nullable: false),
                    DateTeamPicked = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsModded = table.Column<bool>(type: "boolean", nullable: false),
                    BackgroundGuid = table.Column<string>(type: "text", nullable: true),
                    GameVersion = table.Column<int>(type: "integer", nullable: false),
                    LevelType = table.Column<byte>(type: "smallint", nullable: false),
                    StoryId = table.Column<int>(type: "integer", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    IsSubLevel = table.Column<bool>(type: "boolean", nullable: false),
                    IsCopyable = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresMoveController = table.Column<bool>(type: "boolean", nullable: false),
                    Score = table.Column<float>(type: "real", nullable: false),
                    SequentialId = table.Column<int>(type: "integer", nullable: false),
                    PublisherUserId = table.Column<string>(type: "text", nullable: true),
                    OriginalPublisher = table.Column<string>(type: "text", nullable: true),
                    IsReUpload = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameLevels", x => x.LevelId);
                });

            migrationBuilder.CreateTable(
                name: "GameSkillReward",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    RequiredAmount = table.Column<float>(type: "real", nullable: false),
                    GameLevelLevelId = table.Column<int>(type: "integer", nullable: true),
                    GameLevelLevelId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSkillReward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameSkillReward_GameLevels_GameLevelLevelId",
                        column: x => x.GameLevelLevelId,
                        principalTable: "GameLevels",
                        principalColumn: "LevelId");
                    table.ForeignKey(
                        name: "FK_GameSkillReward_GameLevels_GameLevelLevelId1",
                        column: x => x.GameLevelLevelId1,
                        principalTable: "GameLevels",
                        principalColumn: "LevelId");
                });

            migrationBuilder.CreateTable(
                name: "GameSubmittedScores",
                columns: table => new
                {
                    ScoreId = table.Column<string>(type: "text", nullable: false),
                    _Game = table.Column<int>(type: "integer", nullable: false),
                    _Platform = table.Column<int>(type: "integer", nullable: false),
                    LevelId = table.Column<int>(type: "integer", nullable: true),
                    ScoreSubmitted = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    ScoreType = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSubmittedScores", x => x.ScoreId);
                    table.ForeignKey(
                        name: "FK_GameSubmittedScores_GameLevels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "GameLevels",
                        principalColumn: "LevelId");
                });

            migrationBuilder.CreateTable(
                name: "GameUsers",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    EmailAddress = table.Column<string>(type: "text", nullable: true),
                    PasswordBcrypt = table.Column<string>(type: "text", nullable: true),
                    EmailAddressVerified = table.Column<bool>(type: "boolean", nullable: false),
                    ShouldResetPassword = table.Column<bool>(type: "boolean", nullable: false),
                    IconHash = table.Column<string>(type: "text", nullable: false),
                    ForceMatch = table.Column<string>(type: "text", nullable: true),
                    PspIconHash = table.Column<string>(type: "text", nullable: false),
                    VitaIconHash = table.Column<string>(type: "text", nullable: false),
                    BetaIconHash = table.Column<string>(type: "text", nullable: false),
                    FilesizeQuotaUsage = table.Column<int>(type: "integer", nullable: false),
                    TimedLevelUploads = table.Column<int>(type: "integer", nullable: false),
                    TimedLevelUploadExpiryDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    LocationX = table.Column<int>(type: "integer", nullable: false),
                    LocationY = table.Column<int>(type: "integer", nullable: false),
                    JoinDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    BetaPlanetsHash = table.Column<string>(type: "text", nullable: false),
                    Lbp2PlanetsHash = table.Column<string>(type: "text", nullable: false),
                    Lbp3PlanetsHash = table.Column<string>(type: "text", nullable: false),
                    VitaPlanetsHash = table.Column<string>(type: "text", nullable: false),
                    YayFaceHash = table.Column<string>(type: "text", nullable: false),
                    BooFaceHash = table.Column<string>(type: "text", nullable: false),
                    MehFaceHash = table.Column<string>(type: "text", nullable: false),
                    AllowIpAuthentication = table.Column<bool>(type: "boolean", nullable: false),
                    BanReason = table.Column<string>(type: "text", nullable: true),
                    BanExpiryDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastLoginDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RpcnAuthenticationAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    PsnAuthenticationAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    PresenceServerAuthToken = table.Column<string>(type: "text", nullable: true),
                    UnescapeXmlSequences = table.Column<bool>(type: "boolean", nullable: false),
                    ShowModdedContent = table.Column<bool>(type: "boolean", nullable: false),
                    ShowReuploadedContent = table.Column<bool>(type: "boolean", nullable: false),
                    GameSubmittedScoreScoreId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameUsers", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_GameUsers_GameSubmittedScores_GameSubmittedScoreScoreId",
                        column: x => x.GameSubmittedScoreScoreId,
                        principalTable: "GameSubmittedScores",
                        principalColumn: "ScoreId");
                });

            migrationBuilder.CreateTable(
                name: "GameNotifications",
                columns: table => new
                {
                    NotificationId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    FontAwesomeIcon = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameNotifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_GameNotifications_GameUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "GamePhotos",
                columns: table => new
                {
                    PhotoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TakenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PublisherUserId = table.Column<string>(type: "text", nullable: true),
                    LevelName = table.Column<string>(type: "text", nullable: true),
                    LevelType = table.Column<string>(type: "text", nullable: true),
                    LevelId = table.Column<int>(type: "integer", nullable: false),
                    SmallAssetAssetHash = table.Column<string>(type: "text", nullable: true),
                    MediumAssetAssetHash = table.Column<string>(type: "text", nullable: true),
                    LargeAssetAssetHash = table.Column<string>(type: "text", nullable: true),
                    PlanHash = table.Column<string>(type: "text", nullable: true),
                    Subject1UserUserId = table.Column<string>(type: "text", nullable: true),
                    Subject1DisplayName = table.Column<string>(type: "text", nullable: true),
                    Subject2UserUserId = table.Column<string>(type: "text", nullable: true),
                    Subject2DisplayName = table.Column<string>(type: "text", nullable: true),
                    Subject3UserUserId = table.Column<string>(type: "text", nullable: true),
                    Subject3DisplayName = table.Column<string>(type: "text", nullable: true),
                    Subject4UserUserId = table.Column<string>(type: "text", nullable: true),
                    Subject4DisplayName = table.Column<string>(type: "text", nullable: true),
                    SequentialId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePhotos", x => x.PhotoId);
                    table.ForeignKey(
                        name: "FK_GamePhotos_GameAssets_LargeAssetAssetHash",
                        column: x => x.LargeAssetAssetHash,
                        principalTable: "GameAssets",
                        principalColumn: "AssetHash");
                    table.ForeignKey(
                        name: "FK_GamePhotos_GameAssets_MediumAssetAssetHash",
                        column: x => x.MediumAssetAssetHash,
                        principalTable: "GameAssets",
                        principalColumn: "AssetHash");
                    table.ForeignKey(
                        name: "FK_GamePhotos_GameAssets_SmallAssetAssetHash",
                        column: x => x.SmallAssetAssetHash,
                        principalTable: "GameAssets",
                        principalColumn: "AssetHash");
                    table.ForeignKey(
                        name: "FK_GamePhotos_GameLevels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "GameLevels",
                        principalColumn: "LevelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GamePhotos_GameUsers_PublisherUserId",
                        column: x => x.PublisherUserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_GamePhotos_GameUsers_Subject1UserUserId",
                        column: x => x.Subject1UserUserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_GamePhotos_GameUsers_Subject2UserUserId",
                        column: x => x.Subject2UserUserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_GamePhotos_GameUsers_Subject3UserUserId",
                        column: x => x.Subject3UserUserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_GamePhotos_GameUsers_Subject4UserUserId",
                        column: x => x.Subject4UserUserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "GamePlaylists",
                columns: table => new
                {
                    PlaylistId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublisherId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IconHash = table.Column<string>(type: "text", nullable: true),
                    LocationX = table.Column<int>(type: "integer", nullable: false),
                    LocationY = table.Column<int>(type: "integer", nullable: false),
                    CreationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdateDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsRoot = table.Column<bool>(type: "boolean", nullable: false),
                    SequentialId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePlaylists", x => x.PlaylistId);
                    table.ForeignKey(
                        name: "FK_GamePlaylists_GameUsers_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameProfileComments",
                columns: table => new
                {
                    SequentialId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuthorUserId = table.Column<string>(type: "text", nullable: true),
                    ProfileUserId = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameProfileComments", x => x.SequentialId);
                    table.ForeignKey(
                        name: "FK_GameProfileComments_GameUsers_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_GameProfileComments_GameUsers_ProfileUserId",
                        column: x => x.ProfileUserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "GameReviews",
                columns: table => new
                {
                    ReviewId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LevelId = table.Column<int>(type: "integer", nullable: true),
                    PublisherUserId = table.Column<string>(type: "text", nullable: true),
                    PostedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Labels = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    SequentialId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameReviews", x => x.ReviewId);
                    table.ForeignKey(
                        name: "FK_GameReviews_GameLevels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "GameLevels",
                        principalColumn: "LevelId");
                    table.ForeignKey(
                        name: "FK_GameReviews_GameUsers_PublisherUserId",
                        column: x => x.PublisherUserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "GameUserVerifiedIpRelations",
                columns: table => new
                {
                    IpAddress = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameUserVerifiedIpRelations", x => new { x.UserId, x.IpAddress });
                    table.ForeignKey(
                        name: "FK_GameUserVerifiedIpRelations_GameUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LevelCommentRelations",
                columns: table => new
                {
                    CommentRelationId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    CommentSequentialId = table.Column<int>(type: "integer", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelCommentRelations", x => x.CommentRelationId);
                    table.ForeignKey(
                        name: "FK_LevelCommentRelations_GameLevelComments_CommentSequentialId",
                        column: x => x.CommentSequentialId,
                        principalTable: "GameLevelComments",
                        principalColumn: "SequentialId");
                    table.ForeignKey(
                        name: "FK_LevelCommentRelations_GameUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "PinProgressRelations",
                columns: table => new
                {
                    PinId = table.Column<long>(type: "bigint", nullable: false),
                    PublisherId = table.Column<string>(type: "text", nullable: false),
                    Progress = table.Column<int>(type: "integer", nullable: false),
                    FirstPublished = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastUpdated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsBeta = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PinProgressRelations", x => new { x.PinId, x.PublisherId });
                    table.ForeignKey(
                        name: "FK_PinProgressRelations_GameUsers_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayLevelRelations",
                columns: table => new
                {
                    LevelId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayLevelRelations", x => new { x.LevelId, x.UserId });
                    table.ForeignKey(
                        name: "FK_PlayLevelRelations_GameLevels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "GameLevels",
                        principalColumn: "LevelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayLevelRelations_GameUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfilePinRelations",
                columns: table => new
                {
                    PinId = table.Column<long>(type: "bigint", nullable: false),
                    PublisherId = table.Column<string>(type: "text", nullable: false),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    Game = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfilePinRelations", x => new { x.PinId, x.PublisherId });
                    table.ForeignKey(
                        name: "FK_ProfilePinRelations_GameUsers_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QueueLevelRelations",
                columns: table => new
                {
                    LevelId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueLevelRelations", x => new { x.LevelId, x.UserId });
                    table.ForeignKey(
                        name: "FK_QueueLevelRelations_GameLevels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "GameLevels",
                        principalColumn: "LevelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QueueLevelRelations_GameUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RateLevelRelations",
                columns: table => new
                {
                    RateLevelRelationId = table.Column<string>(type: "text", nullable: false),
                    LevelId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RateLevelRelations", x => x.RateLevelRelationId);
                    table.ForeignKey(
                        name: "FK_RateLevelRelations_GameLevels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "GameLevels",
                        principalColumn: "LevelId");
                    table.ForeignKey(
                        name: "FK_RateLevelRelations_GameUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "TagLevelRelations",
                columns: table => new
                {
                    LevelId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    _Tag = table.Column<byte>(type: "smallint", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagLevelRelations", x => new { x._Tag, x.UserId, x.LevelId });
                    table.ForeignKey(
                        name: "FK_TagLevelRelations_GameLevels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "GameLevels",
                        principalColumn: "LevelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TagLevelRelations_GameUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    TokenId = table.Column<string>(type: "text", nullable: false),
                    TokenData = table.Column<string>(type: "text", nullable: true),
                    TokenType = table.Column<int>(type: "integer", nullable: false),
                    TokenPlatform = table.Column<int>(type: "integer", nullable: false),
                    TokenGame = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LoginDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    Digest = table.Column<string>(type: "text", nullable: true),
                    IsHmacDigest = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.TokenId);
                    table.ForeignKey(
                        name: "FK_Tokens_GameUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UniquePlayLevelRelations",
                columns: table => new
                {
                    LevelId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniquePlayLevelRelations", x => new { x.LevelId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UniquePlayLevelRelations_GameLevels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "GameLevels",
                        principalColumn: "LevelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UniquePlayLevelRelations_GameUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LevelPlaylistRelations",
                columns: table => new
                {
                    PlaylistId = table.Column<int>(type: "integer", nullable: false),
                    LevelId = table.Column<int>(type: "integer", nullable: false),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevelPlaylistRelations", x => new { x.PlaylistId, x.LevelId });
                    table.ForeignKey(
                        name: "FK_LevelPlaylistRelations_GameLevels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "GameLevels",
                        principalColumn: "LevelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LevelPlaylistRelations_GamePlaylists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "GamePlaylists",
                        principalColumn: "PlaylistId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubPlaylistRelations",
                columns: table => new
                {
                    PlaylistId = table.Column<int>(type: "integer", nullable: false),
                    SubPlaylistId = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubPlaylistRelations", x => new { x.PlaylistId, x.SubPlaylistId });
                    table.ForeignKey(
                        name: "FK_SubPlaylistRelations_GamePlaylists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "GamePlaylists",
                        principalColumn: "PlaylistId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubPlaylistRelations_GamePlaylists_SubPlaylistId",
                        column: x => x.SubPlaylistId,
                        principalTable: "GamePlaylists",
                        principalColumn: "PlaylistId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileCommentRelations",
                columns: table => new
                {
                    CommentRelationId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    CommentSequentialId = table.Column<int>(type: "integer", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileCommentRelations", x => x.CommentRelationId);
                    table.ForeignKey(
                        name: "FK_ProfileCommentRelations_GameProfileComments_CommentSequenti~",
                        column: x => x.CommentSequentialId,
                        principalTable: "GameProfileComments",
                        principalColumn: "SequentialId");
                    table.ForeignKey(
                        name: "FK_ProfileCommentRelations_GameUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "RateReviewRelations",
                columns: table => new
                {
                    ReviewId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    _ReviewRatingType = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RateReviewRelations", x => new { x.ReviewId, x.UserId });
                    table.ForeignKey(
                        name: "FK_RateReviewRelations_GameReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "GameReviews",
                        principalColumn: "ReviewId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RateReviewRelations_GameUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_UserId1",
                table: "Events",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_FavouriteLevelRelations_UserId",
                table: "FavouriteLevelRelations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FavouritePlaylistRelations_PlaylistId",
                table: "FavouritePlaylistRelations",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_FavouriteUserRelations_UserFavouritingId",
                table: "FavouriteUserRelations",
                column: "UserFavouritingId");

            migrationBuilder.CreateIndex(
                name: "IX_GameAssets_OriginalUploaderUserId",
                table: "GameAssets",
                column: "OriginalUploaderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameChallenges_LevelId",
                table: "GameChallenges",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_GameChallenges_PublisherUserId",
                table: "GameChallenges",
                column: "PublisherUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameChallengeScores_ChallengeId",
                table: "GameChallengeScores",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_GameChallengeScores_PublisherUserId",
                table: "GameChallengeScores",
                column: "PublisherUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameContests_OrganizerUserId",
                table: "GameContests",
                column: "OrganizerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameContests_TemplateLevelLevelId",
                table: "GameContests",
                column: "TemplateLevelLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_GameLevelComments_AuthorUserId",
                table: "GameLevelComments",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameLevelComments_LevelId",
                table: "GameLevelComments",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_GameLevels_PublisherUserId",
                table: "GameLevels",
                column: "PublisherUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameLevels_Title_Description_StoryId",
                table: "GameLevels",
                columns: new[] { "Title", "Description", "StoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_GameNotifications_UserId",
                table: "GameNotifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePhotos_LargeAssetAssetHash",
                table: "GamePhotos",
                column: "LargeAssetAssetHash");

            migrationBuilder.CreateIndex(
                name: "IX_GamePhotos_LevelId",
                table: "GamePhotos",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePhotos_MediumAssetAssetHash",
                table: "GamePhotos",
                column: "MediumAssetAssetHash");

            migrationBuilder.CreateIndex(
                name: "IX_GamePhotos_PublisherUserId",
                table: "GamePhotos",
                column: "PublisherUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePhotos_SmallAssetAssetHash",
                table: "GamePhotos",
                column: "SmallAssetAssetHash");

            migrationBuilder.CreateIndex(
                name: "IX_GamePhotos_Subject1UserUserId",
                table: "GamePhotos",
                column: "Subject1UserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePhotos_Subject2UserUserId",
                table: "GamePhotos",
                column: "Subject2UserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePhotos_Subject3UserUserId",
                table: "GamePhotos",
                column: "Subject3UserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePhotos_Subject4UserUserId",
                table: "GamePhotos",
                column: "Subject4UserUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePlaylists_PublisherId",
                table: "GamePlaylists",
                column: "PublisherId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameProfileComments_AuthorUserId",
                table: "GameProfileComments",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameProfileComments_ProfileUserId",
                table: "GameProfileComments",
                column: "ProfileUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameReviews_LevelId",
                table: "GameReviews",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_GameReviews_PublisherUserId",
                table: "GameReviews",
                column: "PublisherUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSkillReward_GameLevelLevelId",
                table: "GameSkillReward",
                column: "GameLevelLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSkillReward_GameLevelLevelId1",
                table: "GameSkillReward",
                column: "GameLevelLevelId1");

            migrationBuilder.CreateIndex(
                name: "IX_GameSubmittedScores__Game_Score_ScoreType",
                table: "GameSubmittedScores",
                columns: new[] { "_Game", "Score", "ScoreType" });

            migrationBuilder.CreateIndex(
                name: "IX_GameSubmittedScores_LevelId",
                table: "GameSubmittedScores",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_GameUsers_GameSubmittedScoreScoreId",
                table: "GameUsers",
                column: "GameSubmittedScoreScoreId");

            migrationBuilder.CreateIndex(
                name: "IX_GameUsers_Username_EmailAddress_PasswordBcrypt",
                table: "GameUsers",
                columns: new[] { "Username", "EmailAddress", "PasswordBcrypt" });

            migrationBuilder.CreateIndex(
                name: "IX_LevelCommentRelations_CommentSequentialId",
                table: "LevelCommentRelations",
                column: "CommentSequentialId");

            migrationBuilder.CreateIndex(
                name: "IX_LevelCommentRelations_UserId",
                table: "LevelCommentRelations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LevelPlaylistRelations_LevelId",
                table: "LevelPlaylistRelations",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_PinProgressRelations_PublisherId",
                table: "PinProgressRelations",
                column: "PublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayLevelRelations_UserId",
                table: "PlayLevelRelations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileCommentRelations_CommentSequentialId",
                table: "ProfileCommentRelations",
                column: "CommentSequentialId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileCommentRelations_UserId",
                table: "ProfileCommentRelations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfilePinRelations_PublisherId",
                table: "ProfilePinRelations",
                column: "PublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_QueuedRegistrations_Username_EmailAddress",
                table: "QueuedRegistrations",
                columns: new[] { "Username", "EmailAddress" });

            migrationBuilder.CreateIndex(
                name: "IX_QueueLevelRelations_UserId",
                table: "QueueLevelRelations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RateLevelRelations_LevelId",
                table: "RateLevelRelations",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_RateLevelRelations_UserId",
                table: "RateLevelRelations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RateReviewRelations_UserId",
                table: "RateReviewRelations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubPlaylistRelations_SubPlaylistId",
                table: "SubPlaylistRelations",
                column: "SubPlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_TagLevelRelations_LevelId",
                table: "TagLevelRelations",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_TagLevelRelations_UserId",
                table: "TagLevelRelations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_UserId",
                table: "Tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UniquePlayLevelRelations_UserId",
                table: "UniquePlayLevelRelations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailVerificationCodes_GameUsers_UserId",
                table: "EmailVerificationCodes",
                column: "UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_GameUsers_UserId1",
                table: "Events",
                column: "UserId1",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FavouriteLevelRelations_GameLevels_LevelId",
                table: "FavouriteLevelRelations",
                column: "LevelId",
                principalTable: "GameLevels",
                principalColumn: "LevelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FavouriteLevelRelations_GameUsers_UserId",
                table: "FavouriteLevelRelations",
                column: "UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FavouritePlaylistRelations_GamePlaylists_PlaylistId",
                table: "FavouritePlaylistRelations",
                column: "PlaylistId",
                principalTable: "GamePlaylists",
                principalColumn: "PlaylistId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FavouritePlaylistRelations_GameUsers_UserId",
                table: "FavouritePlaylistRelations",
                column: "UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FavouriteUserRelations_GameUsers_UserFavouritingId",
                table: "FavouriteUserRelations",
                column: "UserFavouritingId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FavouriteUserRelations_GameUsers_UserToFavouriteId",
                table: "FavouriteUserRelations",
                column: "UserToFavouriteId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameAssets_GameUsers_OriginalUploaderUserId",
                table: "GameAssets",
                column: "OriginalUploaderUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameChallenges_GameLevels_LevelId",
                table: "GameChallenges",
                column: "LevelId",
                principalTable: "GameLevels",
                principalColumn: "LevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameChallenges_GameUsers_PublisherUserId",
                table: "GameChallenges",
                column: "PublisherUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameChallengeScores_GameUsers_PublisherUserId",
                table: "GameChallengeScores",
                column: "PublisherUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameContests_GameLevels_TemplateLevelLevelId",
                table: "GameContests",
                column: "TemplateLevelLevelId",
                principalTable: "GameLevels",
                principalColumn: "LevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameContests_GameUsers_OrganizerUserId",
                table: "GameContests",
                column: "OrganizerUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameIpVerificationRequests_GameUsers_UserId",
                table: "GameIpVerificationRequests",
                column: "UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameLevelComments_GameLevels_LevelId",
                table: "GameLevelComments",
                column: "LevelId",
                principalTable: "GameLevels",
                principalColumn: "LevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameLevelComments_GameUsers_AuthorUserId",
                table: "GameLevelComments",
                column: "AuthorUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameLevels_GameUsers_PublisherUserId",
                table: "GameLevels",
                column: "PublisherUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameLevels_GameUsers_PublisherUserId",
                table: "GameLevels");

            migrationBuilder.DropTable(
                name: "AssetDependencyRelations");

            migrationBuilder.DropTable(
                name: "DisallowedUsers");

            migrationBuilder.DropTable(
                name: "EmailVerificationCodes");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "FavouriteLevelRelations");

            migrationBuilder.DropTable(
                name: "FavouritePlaylistRelations");

            migrationBuilder.DropTable(
                name: "FavouriteUserRelations");

            migrationBuilder.DropTable(
                name: "GameAnnouncements");

            migrationBuilder.DropTable(
                name: "GameChallengeScores");

            migrationBuilder.DropTable(
                name: "GameContests");

            migrationBuilder.DropTable(
                name: "GameIpVerificationRequests");

            migrationBuilder.DropTable(
                name: "GameNotifications");

            migrationBuilder.DropTable(
                name: "GamePhotos");

            migrationBuilder.DropTable(
                name: "GameSkillReward");

            migrationBuilder.DropTable(
                name: "GameUserVerifiedIpRelations");

            migrationBuilder.DropTable(
                name: "LevelCommentRelations");

            migrationBuilder.DropTable(
                name: "LevelPlaylistRelations");

            migrationBuilder.DropTable(
                name: "PinProgressRelations");

            migrationBuilder.DropTable(
                name: "PlayLevelRelations");

            migrationBuilder.DropTable(
                name: "ProfileCommentRelations");

            migrationBuilder.DropTable(
                name: "ProfilePinRelations");

            migrationBuilder.DropTable(
                name: "QueuedRegistrations");

            migrationBuilder.DropTable(
                name: "QueueLevelRelations");

            migrationBuilder.DropTable(
                name: "RateLevelRelations");

            migrationBuilder.DropTable(
                name: "RateReviewRelations");

            migrationBuilder.DropTable(
                name: "RequestStatistics");

            migrationBuilder.DropTable(
                name: "SequentialIdStorage");

            migrationBuilder.DropTable(
                name: "SubPlaylistRelations");

            migrationBuilder.DropTable(
                name: "TagLevelRelations");

            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.DropTable(
                name: "UniquePlayLevelRelations");

            migrationBuilder.DropTable(
                name: "GameChallenges");

            migrationBuilder.DropTable(
                name: "GameAssets");

            migrationBuilder.DropTable(
                name: "GameLevelComments");

            migrationBuilder.DropTable(
                name: "GameProfileComments");

            migrationBuilder.DropTable(
                name: "GameReviews");

            migrationBuilder.DropTable(
                name: "GamePlaylists");

            migrationBuilder.DropTable(
                name: "GameUsers");

            migrationBuilder.DropTable(
                name: "GameSubmittedScores");

            migrationBuilder.DropTable(
                name: "GameLevels");
        }
    }
}
