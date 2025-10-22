using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20251004094000_AddModerationAttributesToEvents")]
    public partial class AddModerationAttributesToEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalInfo",
                table: "Events",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsModified",
                table: "Events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // No need to set involved user here, as there have never been any private events
            // which involve a non-mod user before
            migrationBuilder.AddColumn<string>(
                name: "InvolvedUserId",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_InvolvedUserId",
                table: "Events",
                column: "InvolvedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_GameUsers_InvolvedUserId",
                table: "Events",
                column: "InvolvedUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalInfo",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "IsModified",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_GameUsers_InvolvedUserId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_InvolvedUserId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "InvolvedUserId",
                table: "Events");
        }
    }
}
