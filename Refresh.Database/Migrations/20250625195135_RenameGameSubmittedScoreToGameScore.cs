using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250625195135_RenameGameSubmittedScoreToGameScore")]
    /// <inheritdoc />
    public partial class RenameGameSubmittedScoreToGameScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("GameSubmittedScores", newName: "GameScores");
            migrationBuilder.RenameIndex("PK_GameSubmittedScores", "PK_GameScores");
            migrationBuilder.RenameIndex("IX_GameSubmittedScores_Game_Score_ScoreType", "IX_GameScores_Game_Score_ScoreType");
            migrationBuilder.RenameIndex("IX_GameSubmittedScores_LevelId", "IX_GameScores_LevelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable("GameScores", newName: "GameSubmittedScores");
            migrationBuilder.RenameIndex(newName: "PK_GameSubmittedScores", name: "PK_GameScores");
            migrationBuilder.RenameIndex(newName: "IX_GameSubmittedScores_Game_Score_ScoreType", name: "IX_GameScores_Game_Score_ScoreType");
            migrationBuilder.RenameIndex(newName: "IX_GameScores_LevelId", name: "IX_GameScores_LevelId");
        }
    }
}
