using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250626043223_AddLowercaseUsernameToQueuedRegistration")]
    public partial class AddLowercaseUsernameToQueuedRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QueuedRegistrations_Username_EmailAddress",
                table: "QueuedRegistrations");

            migrationBuilder.AddColumn<string>(
                name: "UsernameLower",
                table: "QueuedRegistrations",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QueuedRegistrations_Username_UsernameLower_EmailAddress",
                table: "QueuedRegistrations",
                columns: new[] { "Username", "UsernameLower", "EmailAddress" });

            migrationBuilder.Sql("UPDATE \"QueuedRegistrations\" SET \"UsernameLower\" = lower(\"Username\")");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QueuedRegistrations_Username_UsernameLower_EmailAddress",
                table: "QueuedRegistrations");

            migrationBuilder.DropColumn(
                name: "UsernameLower",
                table: "QueuedRegistrations");

            migrationBuilder.CreateIndex(
                name: "IX_QueuedRegistrations_Username_EmailAddress",
                table: "QueuedRegistrations",
                columns: new[] { "Username", "EmailAddress" });
        }
    }
}
