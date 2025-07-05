using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    public partial class ProfilePinRelationUpdatingFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfilePinRelations",
                table: "ProfilePinRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfilePinRelations",
                table: "ProfilePinRelations",
                columns: new[] { "Index", "PublisherId", "Game" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfilePinRelations",
                table: "ProfilePinRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfilePinRelations",
                table: "ProfilePinRelations",
                columns: new[] { "PinId", "PublisherId" });
        }
    }
}
