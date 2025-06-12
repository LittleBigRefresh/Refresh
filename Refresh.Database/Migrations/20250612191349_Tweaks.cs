using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250612191349_Tweaks")]
    public partial class Tweaks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_GameUsers_UserId1",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "UserId1",
                table: "Events",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Events_UserId1",
                table: "Events",
                newName: "IX_Events_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_GameUsers_UserId",
                table: "Events",
                column: "UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_GameUsers_UserId",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Events",
                newName: "UserId1");

            migrationBuilder.RenameIndex(
                name: "IX_Events_UserId",
                table: "Events",
                newName: "IX_Events_UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_GameUsers_UserId1",
                table: "Events",
                column: "UserId1",
                principalTable: "GameUsers",
                principalColumn: "UserId");
        }
    }
}
