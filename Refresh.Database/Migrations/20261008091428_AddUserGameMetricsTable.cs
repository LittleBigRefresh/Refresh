using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20261008091428_AddUserGameMetricsTable")]
    public partial class AddUserGameMetricsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TotalPlayTimeMinutes",
                table: "GameUserStatistics",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "UserGameMetrics",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Game = table.Column<int>(type: "integer", nullable: false),
                    Platform = table.Column<int>(type: "integer", nullable: false),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastRoomUpdateAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TotalPlayTimeMinutes = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGameMetrics", x => new { x.UserId, x.Game, x.Platform });
                    table.ForeignKey(
                        name: "FK_UserGameMetrics_GameUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserGameMetrics");

            migrationBuilder.DropColumn(
                name: "TotalPlayTimeMinutes",
                table: "GameUserStatistics");
        }
    }
}
