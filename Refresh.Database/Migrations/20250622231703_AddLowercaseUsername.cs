using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250622231703_AddLowercaseUsername")]
    public partial class AddLowercaseUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameUsers_Username_EmailAddress_PasswordBcrypt",
                table: "GameUsers");

            migrationBuilder.AddColumn<string>(
                name: "UsernameLower",
                table: "GameUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_GameUsers_Username_UsernameLower_EmailAddress_PasswordBcrypt",
                table: "GameUsers",
                columns: new[] { "Username", "UsernameLower", "EmailAddress", "PasswordBcrypt" });

            migrationBuilder.Sql("UPDATE \"GameUsers\" SET \"UsernameLower\" = lower(\"Username\") ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameUsers_Username_UsernameLower_EmailAddress_PasswordBcrypt",
                table: "GameUsers");

            migrationBuilder.DropColumn(
                name: "UsernameLower",
                table: "GameUsers");

            migrationBuilder.CreateIndex(
                name: "IX_GameUsers_Username_EmailAddress_PasswordBcrypt",
                table: "GameUsers",
                columns: new[] { "Username", "EmailAddress", "PasswordBcrypt" });
        }
    }
}
