using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20251007182051_ProperlyAddScorePlayerList")]
    public partial class ProperlyAddScorePlayerList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublisherId",
                table: "GameScores",
                type: "text",
                nullable: false,
                defaultValue: "");
            
            // SQL to copy publisher ID from player list to publisher ID attribute
            migrationBuilder.Sql("UPDATE \"GameScores\" SET \"PublisherId\" = \"PlayerIdsRaw\"[1] WHERE \"PlayerIdsRaw\"[1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GameScores_PublisherId",
                table: "GameScores",
                column: "PublisherId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameScores_GameUsers_PublisherId",
                table: "GameScores",
                column: "PublisherId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameScores_GameUsers_PublisherId",
                table: "GameScores");

            migrationBuilder.DropIndex(
                name: "IX_GameScores_PublisherId",
                table: "GameScores");

            migrationBuilder.DropColumn(
                name: "PublisherId",
                table: "GameScores");
        }
    }
}
