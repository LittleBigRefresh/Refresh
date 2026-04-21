using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20260421181503_SplitUploadRateLimitsToSeparateTable")]
    public partial class SplitUploadRateLimitsToSeparateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // probably no need to migrate these, they're temporary anyway
            migrationBuilder.DropColumn(
                name: "TimedLevelUploadExpiryDate",
                table: "GameUsers");

            migrationBuilder.DropColumn(
                name: "TimedLevelUploads",
                table: "GameUsers");

            migrationBuilder.CreateTable(
                name: "EntityUploadRateLimits",
                columns: table => new
                {
                    Entity = table.Column<byte>(type: "smallint", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    EntityQuota = table.Column<int>(type: "integer", nullable: false),
                    ExpiryDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityUploadRateLimits", x => new { x.UserId, x.Entity });
                    table.ForeignKey(
                        name: "FK_EntityUploadRateLimits_GameUsers_UserId",
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
                name: "EntityUploadRateLimits");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "TimedLevelUploadExpiryDate",
                table: "GameUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimedLevelUploads",
                table: "GameUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
