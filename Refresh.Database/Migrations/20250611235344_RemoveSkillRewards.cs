using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250611235344_RemoveSkillRewards")]
    public partial class RemoveSkillRewards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameSkillReward_GameLevels_GameLevelLevelId1",
                table: "GameSkillReward");

            migrationBuilder.DropIndex(
                name: "IX_GameSkillReward_GameLevelLevelId1",
                table: "GameSkillReward");

            migrationBuilder.DropColumn(
                name: "GameLevelLevelId1",
                table: "GameSkillReward");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GameLevelLevelId1",
                table: "GameSkillReward",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameSkillReward_GameLevelLevelId1",
                table: "GameSkillReward",
                column: "GameLevelLevelId1");

            migrationBuilder.AddForeignKey(
                name: "FK_GameSkillReward_GameLevels_GameLevelLevelId1",
                table: "GameSkillReward",
                column: "GameLevelLevelId1",
                principalTable: "GameLevels",
                principalColumn: "LevelId");
        }
    }
}
