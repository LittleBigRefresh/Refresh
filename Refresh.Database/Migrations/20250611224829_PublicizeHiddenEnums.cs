using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250611224829_PublicizeHiddenEnums")]
    public partial class PublicizeHiddenEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "_TokenGame",
                table: "Tokens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "_TokenPlatform",
                table: "Tokens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "_TokenType",
                table: "Tokens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "_RatingType",
                table: "RateLevelRelations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "_Game",
                table: "ProfilePinRelations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "_RatingType",
                table: "ProfileCommentRelations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "_RatingType",
                table: "LevelCommentRelations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte>(
                name: "_Role",
                table: "GameUsers",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<int>(
                name: "_ConditionType",
                table: "GameSkillReward",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "_GameVersion",
                table: "GameLevels",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "_LevelType",
                table: "GameLevels",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "_AssetSerializationMethod",
                table: "GameAssets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "_AssetType",
                table: "GameAssets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "_EventType",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "_StoredDataType",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "_TokenGame",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "_TokenPlatform",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "_TokenType",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "_RatingType",
                table: "RateLevelRelations");

            migrationBuilder.DropColumn(
                name: "_Game",
                table: "ProfilePinRelations");

            migrationBuilder.DropColumn(
                name: "_RatingType",
                table: "ProfileCommentRelations");

            migrationBuilder.DropColumn(
                name: "_RatingType",
                table: "LevelCommentRelations");

            migrationBuilder.DropColumn(
                name: "_Role",
                table: "GameUsers");

            migrationBuilder.DropColumn(
                name: "_ConditionType",
                table: "GameSkillReward");

            migrationBuilder.DropColumn(
                name: "_GameVersion",
                table: "GameLevels");

            migrationBuilder.DropColumn(
                name: "_LevelType",
                table: "GameLevels");

            migrationBuilder.DropColumn(
                name: "_AssetSerializationMethod",
                table: "GameAssets");

            migrationBuilder.DropColumn(
                name: "_AssetType",
                table: "GameAssets");

            migrationBuilder.DropColumn(
                name: "_EventType",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "_StoredDataType",
                table: "Events");
        }
    }
}
