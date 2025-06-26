using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250626004526_AddUserStatistics")]
    public partial class AddUserStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StatisticsUserId",
                table: "GameUsers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GameUserStatistics",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RecalculateAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FavouriteCount = table.Column<int>(type: "integer", nullable: false),
                    CommentCount = table.Column<int>(type: "integer", nullable: false),
                    LevelCount = table.Column<int>(type: "integer", nullable: false),
                    PhotosByUserCount = table.Column<int>(type: "integer", nullable: false),
                    PhotosWithUserCount = table.Column<int>(type: "integer", nullable: false),
                    ReviewCount = table.Column<int>(type: "integer", nullable: false),
                    FavouriteLevelCount = table.Column<int>(type: "integer", nullable: false),
                    FavouriteUserCount = table.Column<int>(type: "integer", nullable: false),
                    QueueCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameUserStatistics", x => x.UserId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameUsers_StatisticsUserId",
                table: "GameUsers",
                column: "StatisticsUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameUsers_GameUserStatistics_StatisticsUserId",
                table: "GameUsers",
                column: "StatisticsUserId",
                principalTable: "GameUserStatistics",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameUsers_GameUserStatistics_StatisticsUserId",
                table: "GameUsers");

            migrationBuilder.DropTable(
                name: "GameUserStatistics");

            migrationBuilder.DropIndex(
                name: "IX_GameUsers_StatisticsUserId",
                table: "GameUsers");

            migrationBuilder.DropColumn(
                name: "StatisticsUserId",
                table: "GameUsers");
        }
    }
}
