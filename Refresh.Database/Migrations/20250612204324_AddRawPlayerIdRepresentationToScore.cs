using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250612204324_AddRawPlayerIdRepresentationToScore")]
    public partial class AddRawPlayerIdRepresentationToScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameUsers_GameSubmittedScores_GameSubmittedScoreScoreId",
                table: "GameUsers");

            migrationBuilder.DropIndex(
                name: "IX_GameUsers_GameSubmittedScoreScoreId",
                table: "GameUsers");

            migrationBuilder.DropColumn(
                name: "GameSubmittedScoreScoreId",
                table: "GameUsers");

            migrationBuilder.AddColumn<List<string>>(
                name: "PlayerIdsRaw",
                table: "GameSubmittedScores",
                type: "text[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerIdsRaw",
                table: "GameSubmittedScores");

            migrationBuilder.AddColumn<string>(
                name: "GameSubmittedScoreScoreId",
                table: "GameUsers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameUsers_GameSubmittedScoreScoreId",
                table: "GameUsers",
                column: "GameSubmittedScoreScoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameUsers_GameSubmittedScores_GameSubmittedScoreScoreId",
                table: "GameUsers",
                column: "GameSubmittedScoreScoreId",
                principalTable: "GameSubmittedScores",
                principalColumn: "ScoreId");
        }
    }
}
