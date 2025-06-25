using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250625192822_CleanupRealmEnumWorkarounds")]
    /// <inheritdoc />
    public partial class CleanupRealmEnumWorkarounds : Migration
    {
        private void MapFromEnum(MigrationBuilder migration, string name, string table, bool smallInt = true, [CanBeNull] string srcName = null)
        {
            if (srcName == null)
                srcName = '_' + name;

            migration.RenameColumn(srcName, table, name);

            if (smallInt)
                migration.AlterColumn<short>(name, table, type: "smallint");
        }
        
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migration)
        {
            migration.DropColumn(
                name: "TokenGame",
                table: "Tokens");

            migration.DropColumn(
                name: "TokenPlatform",
                table: "Tokens");

            migration.DropColumn(
                name: "TokenType",
                table: "Tokens");

            this.MapFromEnum(migration, "TokenGame", "Tokens", false);
            this.MapFromEnum(migration, "TokenPlatform", "Tokens", false);
            this.MapFromEnum(migration, "TokenType", "Tokens", false);
            
            this.MapFromEnum(migration, "RatingType", "RateReviewRelations", srcName: "_ReviewRatingType");
            this.MapFromEnum(migration, "RatingType", "RateLevelRelations");

            migration.DropColumn(
                name: "Game",
                table: "ProfilePinRelations");
            
            this.MapFromEnum(migration, "Game", "ProfilePinRelations", false);

            this.MapFromEnum(migration, "RatingType", "ProfileCommentRelations");
            this.MapFromEnum(migration, "RatingType", "LevelCommentRelations");

            migration.DropColumn(
                name: "GameVersion",
                table: "GameLevels");

            migration.DropColumn(
                name: "LevelType",
                table: "GameLevels");
            
            this.MapFromEnum(migration, "GameVersion", "GameLevels", false);
            this.MapFromEnum(migration, "LevelType", "GameLevels");

            migration.DropColumn(
                name: "Type",
                table: "GameChallenges");

            this.MapFromEnum(migration, "Type", "GameChallenges", false);
            
            this.MapFromEnum(migration, "AssetFormat", "GameAssets", srcName: "_AssetSerializationMethod");
            
            this.MapFromEnum(migration, "EventType", "Events");

            migration.RenameColumn(
                name: "_Tag",
                table: "TagLevelRelations",
                newName: "Tag");

            migration.RenameColumn(
                name: "_Role",
                table: "GameUsers",
                newName: "Role");

            migration.RenameColumn(
                name: "_Platform",
                table: "GameSubmittedScores",
                newName: "Platform");

            migration.RenameColumn(
                name: "_Game",
                table: "GameSubmittedScores",
                newName: "Game");

            migration.RenameIndex(
                name: "IX_GameSubmittedScores__Game_Score_ScoreType",
                table: "GameSubmittedScores",
                newName: "IX_GameSubmittedScores_Game_Score_ScoreType");

            migration.RenameColumn(
                name: "_ConditionType",
                table: "GameSkillRewards",
                newName: "ConditionType");

            migration.RenameColumn(
                name: "_AssetType",
                table: "GameAssets",
                newName: "AssetType");

            migration.RenameColumn(
                name: "_StoredDataType",
                table: "Events",
                newName: "StoredDataType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new Exception($"Downgrading is not supported with this {nameof(CleanupRealmEnumWorkarounds)}.");
        }
    }
}
