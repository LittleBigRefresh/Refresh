using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Refresh.Database.Models.Activity;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20251022113006_AddEventOverType")]
    public partial class AddEventOverType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "OverType",
                table: "Events",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)EventOverType.Activity);

            // Even though events have never been defined to be private before, we can
            // easily go the extra step of migrating this information anyway.
            migrationBuilder.Sql($"UPDATE \"Events\" SET \"OverType\" = {(byte)EventOverType.Moderation} WHERE \"IsPrivate\" = true");

            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "Events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "Events",
                type: "boolean",
                nullable: false,
                defaultValue: false);
            
            migrationBuilder.Sql($"UPDATE \"Events\" SET \"IsPrivate\" = true WHERE \"OverType\" != {(byte)EventOverType.Activity}");

            migrationBuilder.DropColumn(
                name: "OverType",
                table: "Events");
        }
    }
}
