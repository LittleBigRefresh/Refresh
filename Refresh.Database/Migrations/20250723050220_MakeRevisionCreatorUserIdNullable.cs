using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250723050220_MakeRevisionCreatorUserIdNullable")]
    /// <inheritdoc />
    public partial class MakeRevisionCreatorUserIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameLevelRevisions_GameUsers_CreatedById",
                table: "GameLevelRevisions");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedById",
                table: "GameLevelRevisions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_GameLevelRevisions_GameUsers_CreatedById",
                table: "GameLevelRevisions",
                column: "CreatedById",
                principalTable: "GameUsers",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameLevelRevisions_GameUsers_CreatedById",
                table: "GameLevelRevisions");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedById",
                table: "GameLevelRevisions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GameLevelRevisions_GameUsers_CreatedById",
                table: "GameLevelRevisions",
                column: "CreatedById",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
