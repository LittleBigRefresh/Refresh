using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250616193028_AddIsBetaToPinProgressKey")]
    public partial class AddIsBetaToPinProgressKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PinProgressRelations",
                table: "PinProgressRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PinProgressRelations",
                table: "PinProgressRelations",
                columns: new[] { "PinId", "PublisherId", "IsBeta" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PinProgressRelations",
                table: "PinProgressRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PinProgressRelations",
                table: "PinProgressRelations",
                columns: new[] { "PinId", "PublisherId" });
        }
    }
}
