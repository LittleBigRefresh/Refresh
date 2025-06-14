using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250613235328_ExplicitlyDefineRequiredRelationships")]
    public partial class ExplicitlyDefineRequiredRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_GameUsers_UserId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_GameChallenges_GameLevels_LevelId",
                table: "GameChallenges");

            migrationBuilder.DropForeignKey(
                name: "FK_GameChallengeScores_GameChallenges_ChallengeId",
                table: "GameChallengeScores");

            migrationBuilder.DropForeignKey(
                name: "FK_GameChallengeScores_GameUsers_PublisherUserId",
                table: "GameChallengeScores");

            migrationBuilder.DropForeignKey(
                name: "FK_GameContests_GameUsers_OrganizerUserId",
                table: "GameContests");

            migrationBuilder.DropForeignKey(
                name: "FK_GameLevelComments_GameLevels_LevelId",
                table: "GameLevelComments");

            migrationBuilder.DropForeignKey(
                name: "FK_GameLevelComments_GameUsers_AuthorUserId",
                table: "GameLevelComments");

            migrationBuilder.DropForeignKey(
                name: "FK_GameNotifications_GameUsers_UserId",
                table: "GameNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameAssets_LargeAssetAssetHash",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameAssets_MediumAssetAssetHash",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameAssets_SmallAssetAssetHash",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameUsers_PublisherUserId",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GameProfileComments_GameUsers_AuthorUserId",
                table: "GameProfileComments");

            migrationBuilder.DropForeignKey(
                name: "FK_GameProfileComments_GameUsers_ProfileUserId",
                table: "GameProfileComments");

            migrationBuilder.DropForeignKey(
                name: "FK_GameReviews_GameLevels_LevelId",
                table: "GameReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_GameReviews_GameUsers_PublisherUserId",
                table: "GameReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_GameSubmittedScores_GameLevels_LevelId",
                table: "GameSubmittedScores");

            migrationBuilder.DropForeignKey(
                name: "FK_LevelCommentRelations_GameLevelComments_CommentSequentialId",
                table: "LevelCommentRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_LevelCommentRelations_GameUsers_UserId",
                table: "LevelCommentRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfileCommentRelations_GameProfileComments_CommentSequenti~",
                table: "ProfileCommentRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfileCommentRelations_GameUsers_UserId",
                table: "ProfileCommentRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_RateLevelRelations_GameLevels_LevelId",
                table: "RateLevelRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_RateLevelRelations_GameUsers_UserId",
                table: "RateLevelRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_Tokens_GameUsers_UserId",
                table: "Tokens");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Tokens",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "Tokens",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "RateLevelRelations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LevelId",
                table: "RateLevelRelations",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ProfileCommentRelations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CommentSequentialId",
                table: "ProfileCommentRelations",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "LevelCommentRelations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CommentSequentialId",
                table: "LevelCommentRelations",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LevelId",
                table: "GameSubmittedScores",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PublisherUserId",
                table: "GameReviews",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LevelId",
                table: "GameReviews",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProfileUserId",
                table: "GameProfileComments",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AuthorUserId",
                table: "GameProfileComments",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SmallAssetAssetHash",
                table: "GamePhotos",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PublisherUserId",
                table: "GamePhotos",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MediumAssetAssetHash",
                table: "GamePhotos",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LargeAssetAssetHash",
                table: "GamePhotos",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "GameNotifications",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LevelId",
                table: "GameLevelComments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AuthorUserId",
                table: "GameLevelComments",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OrganizerUserId",
                table: "GameContests",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PublisherUserId",
                table: "GameChallengeScores",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ChallengeId",
                table: "GameChallengeScores",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LevelId",
                table: "GameChallenges",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Events",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_GameUsers_UserId",
                table: "Events",
                column: "UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameChallenges_GameLevels_LevelId",
                table: "GameChallenges",
                column: "LevelId",
                principalTable: "GameLevels",
                principalColumn: "LevelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameChallengeScores_GameChallenges_ChallengeId",
                table: "GameChallengeScores",
                column: "ChallengeId",
                principalTable: "GameChallenges",
                principalColumn: "ChallengeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameChallengeScores_GameUsers_PublisherUserId",
                table: "GameChallengeScores",
                column: "PublisherUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameContests_GameUsers_OrganizerUserId",
                table: "GameContests",
                column: "OrganizerUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameLevelComments_GameLevels_LevelId",
                table: "GameLevelComments",
                column: "LevelId",
                principalTable: "GameLevels",
                principalColumn: "LevelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameLevelComments_GameUsers_AuthorUserId",
                table: "GameLevelComments",
                column: "AuthorUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameNotifications_GameUsers_UserId",
                table: "GameNotifications",
                column: "UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameAssets_LargeAssetAssetHash",
                table: "GamePhotos",
                column: "LargeAssetAssetHash",
                principalTable: "GameAssets",
                principalColumn: "AssetHash",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameAssets_MediumAssetAssetHash",
                table: "GamePhotos",
                column: "MediumAssetAssetHash",
                principalTable: "GameAssets",
                principalColumn: "AssetHash",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameAssets_SmallAssetAssetHash",
                table: "GamePhotos",
                column: "SmallAssetAssetHash",
                principalTable: "GameAssets",
                principalColumn: "AssetHash",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameUsers_PublisherUserId",
                table: "GamePhotos",
                column: "PublisherUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameProfileComments_GameUsers_AuthorUserId",
                table: "GameProfileComments",
                column: "AuthorUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameProfileComments_GameUsers_ProfileUserId",
                table: "GameProfileComments",
                column: "ProfileUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameReviews_GameLevels_LevelId",
                table: "GameReviews",
                column: "LevelId",
                principalTable: "GameLevels",
                principalColumn: "LevelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameReviews_GameUsers_PublisherUserId",
                table: "GameReviews",
                column: "PublisherUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GameSubmittedScores_GameLevels_LevelId",
                table: "GameSubmittedScores",
                column: "LevelId",
                principalTable: "GameLevels",
                principalColumn: "LevelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LevelCommentRelations_GameLevelComments_CommentSequentialId",
                table: "LevelCommentRelations",
                column: "CommentSequentialId",
                principalTable: "GameLevelComments",
                principalColumn: "SequentialId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LevelCommentRelations_GameUsers_UserId",
                table: "LevelCommentRelations",
                column: "UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileCommentRelations_GameProfileComments_CommentSequenti~",
                table: "ProfileCommentRelations",
                column: "CommentSequentialId",
                principalTable: "GameProfileComments",
                principalColumn: "SequentialId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileCommentRelations_GameUsers_UserId",
                table: "ProfileCommentRelations",
                column: "UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RateLevelRelations_GameLevels_LevelId",
                table: "RateLevelRelations",
                column: "LevelId",
                principalTable: "GameLevels",
                principalColumn: "LevelId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RateLevelRelations_GameUsers_UserId",
                table: "RateLevelRelations",
                column: "UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tokens_GameUsers_UserId",
                table: "Tokens",
                column: "UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_GameUsers_UserId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_GameChallenges_GameLevels_LevelId",
                table: "GameChallenges");

            migrationBuilder.DropForeignKey(
                name: "FK_GameChallengeScores_GameChallenges_ChallengeId",
                table: "GameChallengeScores");

            migrationBuilder.DropForeignKey(
                name: "FK_GameChallengeScores_GameUsers_PublisherUserId",
                table: "GameChallengeScores");

            migrationBuilder.DropForeignKey(
                name: "FK_GameContests_GameUsers_OrganizerUserId",
                table: "GameContests");

            migrationBuilder.DropForeignKey(
                name: "FK_GameLevelComments_GameLevels_LevelId",
                table: "GameLevelComments");

            migrationBuilder.DropForeignKey(
                name: "FK_GameLevelComments_GameUsers_AuthorUserId",
                table: "GameLevelComments");

            migrationBuilder.DropForeignKey(
                name: "FK_GameNotifications_GameUsers_UserId",
                table: "GameNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameAssets_LargeAssetAssetHash",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameAssets_MediumAssetAssetHash",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameAssets_SmallAssetAssetHash",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameUsers_PublisherUserId",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GameProfileComments_GameUsers_AuthorUserId",
                table: "GameProfileComments");

            migrationBuilder.DropForeignKey(
                name: "FK_GameProfileComments_GameUsers_ProfileUserId",
                table: "GameProfileComments");

            migrationBuilder.DropForeignKey(
                name: "FK_GameReviews_GameLevels_LevelId",
                table: "GameReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_GameReviews_GameUsers_PublisherUserId",
                table: "GameReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_GameSubmittedScores_GameLevels_LevelId",
                table: "GameSubmittedScores");

            migrationBuilder.DropForeignKey(
                name: "FK_LevelCommentRelations_GameLevelComments_CommentSequentialId",
                table: "LevelCommentRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_LevelCommentRelations_GameUsers_UserId",
                table: "LevelCommentRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfileCommentRelations_GameProfileComments_CommentSequenti~",
                table: "ProfileCommentRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfileCommentRelations_GameUsers_UserId",
                table: "ProfileCommentRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_RateLevelRelations_GameLevels_LevelId",
                table: "RateLevelRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_RateLevelRelations_GameUsers_UserId",
                table: "RateLevelRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_Tokens_GameUsers_UserId",
                table: "Tokens");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Tokens",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "IpAddress",
                table: "Tokens",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "RateLevelRelations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "LevelId",
                table: "RateLevelRelations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ProfileCommentRelations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "CommentSequentialId",
                table: "ProfileCommentRelations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "LevelCommentRelations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "CommentSequentialId",
                table: "LevelCommentRelations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "LevelId",
                table: "GameSubmittedScores",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "PublisherUserId",
                table: "GameReviews",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "LevelId",
                table: "GameReviews",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "ProfileUserId",
                table: "GameProfileComments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "AuthorUserId",
                table: "GameProfileComments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "SmallAssetAssetHash",
                table: "GamePhotos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PublisherUserId",
                table: "GamePhotos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "MediumAssetAssetHash",
                table: "GamePhotos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LargeAssetAssetHash",
                table: "GamePhotos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "GameNotifications",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "LevelId",
                table: "GameLevelComments",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "AuthorUserId",
                table: "GameLevelComments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "OrganizerUserId",
                table: "GameContests",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PublisherUserId",
                table: "GameChallengeScores",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "ChallengeId",
                table: "GameChallengeScores",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "LevelId",
                table: "GameChallenges",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Events",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_GameUsers_UserId",
                table: "Events",
                column: "UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameChallenges_GameLevels_LevelId",
                table: "GameChallenges",
                column: "LevelId",
                principalTable: "GameLevels",
                principalColumn: "LevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameChallengeScores_GameChallenges_ChallengeId",
                table: "GameChallengeScores",
                column: "ChallengeId",
                principalTable: "GameChallenges",
                principalColumn: "ChallengeId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameChallengeScores_GameUsers_PublisherUserId",
                table: "GameChallengeScores",
                column: "PublisherUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameContests_GameUsers_OrganizerUserId",
                table: "GameContests",
                column: "OrganizerUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

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
                name: "FK_GameNotifications_GameUsers_UserId",
                table: "GameNotifications",
                column: "UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameAssets_LargeAssetAssetHash",
                table: "GamePhotos",
                column: "LargeAssetAssetHash",
                principalTable: "GameAssets",
                principalColumn: "AssetHash");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameAssets_MediumAssetAssetHash",
                table: "GamePhotos",
                column: "MediumAssetAssetHash",
                principalTable: "GameAssets",
                principalColumn: "AssetHash");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameAssets_SmallAssetAssetHash",
                table: "GamePhotos",
                column: "SmallAssetAssetHash",
                principalTable: "GameAssets",
                principalColumn: "AssetHash");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameUsers_PublisherUserId",
                table: "GamePhotos",
                column: "PublisherUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameProfileComments_GameUsers_AuthorUserId",
                table: "GameProfileComments",
                column: "AuthorUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameProfileComments_GameUsers_ProfileUserId",
                table: "GameProfileComments",
                column: "ProfileUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameReviews_GameLevels_LevelId",
                table: "GameReviews",
                column: "LevelId",
                principalTable: "GameLevels",
                principalColumn: "LevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameReviews_GameUsers_PublisherUserId",
                table: "GameReviews",
                column: "PublisherUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameSubmittedScores_GameLevels_LevelId",
                table: "GameSubmittedScores",
                column: "LevelId",
                principalTable: "GameLevels",
                principalColumn: "LevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_LevelCommentRelations_GameLevelComments_CommentSequentialId",
                table: "LevelCommentRelations",
                column: "CommentSequentialId",
                principalTable: "GameLevelComments",
                principalColumn: "SequentialId");

            migrationBuilder.AddForeignKey(
                name: "FK_LevelCommentRelations_GameUsers_UserId",
                table: "LevelCommentRelations",
                column: "UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileCommentRelations_GameProfileComments_CommentSequenti~",
                table: "ProfileCommentRelations",
                column: "CommentSequentialId",
                principalTable: "GameProfileComments",
                principalColumn: "SequentialId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileCommentRelations_GameUsers_UserId",
                table: "ProfileCommentRelations",
                column: "UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RateLevelRelations_GameLevels_LevelId",
                table: "RateLevelRelations",
                column: "LevelId",
                principalTable: "GameLevels",
                principalColumn: "LevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_RateLevelRelations_GameUsers_UserId",
                table: "RateLevelRelations",
                column: "UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tokens_GameUsers_UserId",
                table: "Tokens",
                column: "UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");
        }
    }
}
