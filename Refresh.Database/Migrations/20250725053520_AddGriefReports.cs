using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddGriefReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RedirectGriefReportsToPhotos",
                table: "GameUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReporterUserId = table.Column<string>(type: "text", nullable: false),
                    ReportDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GameVersion = table.Column<int>(type: "integer", nullable: false),
                    LevelId = table.Column<int>(type: "integer", nullable: true),
                    LevelType = table.Column<string>(type: "text", nullable: false),
                    InitialStateHash = table.Column<string>(type: "text", nullable: false),
                    GriefStateHash = table.Column<string>(type: "text", nullable: false),
                    PhotoAssetHash = table.Column<string>(type: "text", nullable: false),
                    MarkerRect = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ReviewedByUserId = table.Column<string>(type: "text", nullable: true),
                    ReviewedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ModeratorNotes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_Reports_GameLevels_LevelId",
                        column: x => x.LevelId,
                        principalTable: "GameLevels",
                        principalColumn: "LevelId");
                    table.ForeignKey(
                        name: "FK_Reports_GameUsers_ReporterUserId",
                        column: x => x.ReporterUserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reports_GameUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ReportPlayersRelations",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    IsReporter = table.Column<bool>(type: "boolean", nullable: false),
                    IsInGameNow = table.Column<bool>(type: "boolean", nullable: false),
                    PlayerNumber = table.Column<int>(type: "integer", nullable: false),
                    PlayerRect = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportPlayersRelations", x => new { x.ReportId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ReportPlayersRelations_GameUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportPlayersRelations_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "ReportId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportPlayersRelations_UserId",
                table: "ReportPlayersRelations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_LevelId",
                table: "Reports",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReporterUserId",
                table: "Reports",
                column: "ReporterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReviewedByUserId",
                table: "Reports",
                column: "ReviewedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportPlayersRelations");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropColumn(
                name: "RedirectGriefReportsToPhotos",
                table: "GameUsers");
        }
    }
}
