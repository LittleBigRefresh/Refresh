using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250614045619_RemoveRealmWeirdness")]
    public partial class RemoveRealmWeirdness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameSkillReward_GameLevels_GameLevelLevelId",
                table: "GameSkillReward");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameSkillReward",
                table: "GameSkillReward");

            migrationBuilder.DropIndex(
                name: "IX_GameSkillReward_GameLevelLevelId",
                table: "GameSkillReward");

            migrationBuilder.DropColumn(
                name: "GameLevelLevelId",
                table: "GameSkillReward");

            migrationBuilder.RenameTable(
                name: "GameSkillReward",
                newName: "GameSkillRewards");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "GameSkillRewards",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "LevelId",
                table: "GameSkillRewards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameSkillRewards",
                table: "GameSkillRewards",
                columns: new[] { "LevelId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_GameSkillRewards_GameLevels_LevelId",
                table: "GameSkillRewards",
                column: "LevelId",
                principalTable: "GameLevels",
                principalColumn: "LevelId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameSkillRewards_GameLevels_LevelId",
                table: "GameSkillRewards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GameSkillRewards",
                table: "GameSkillRewards");

            migrationBuilder.DropColumn(
                name: "LevelId",
                table: "GameSkillRewards");

            migrationBuilder.RenameTable(
                name: "GameSkillRewards",
                newName: "GameSkillReward");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "GameSkillReward",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "GameLevelLevelId",
                table: "GameSkillReward",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameSkillReward",
                table: "GameSkillReward",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameSkillReward_GameLevelLevelId",
                table: "GameSkillReward",
                column: "GameLevelLevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameSkillReward_GameLevels_GameLevelLevelId",
                table: "GameSkillReward",
                column: "GameLevelLevelId",
                principalTable: "GameLevels",
                principalColumn: "LevelId");
        }
    }
}
