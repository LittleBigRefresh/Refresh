using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250721211144_AddRevisionTable")]
    /// <inheritdoc />
    public partial class AddRevisionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameLevelRevisions",
                columns: table => new
                {
                    LevelId = table.Column<int>(type: "integer", nullable: false),
                    RevisionId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    IconHash = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    RootResource = table.Column<string>(type: "text", nullable: false),
                    GameVersion = table.Column<int>(type: "integer", nullable: false),
                    LevelType = table.Column<byte>(type: "smallint", nullable: false),
                    StoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameLevelRevisions", x => new { x.LevelId, x.RevisionId });
                    table.ForeignKey(
                        name: "FK_GameLevelRevisions_GameLevels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "GameLevels",
                        principalColumn: "LevelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameLevelRevisions_GameUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "GameUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameLevelRevisions_CreatedById",
                table: "GameLevelRevisions",
                column: "CreatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameLevelRevisions");
        }
    }
}
