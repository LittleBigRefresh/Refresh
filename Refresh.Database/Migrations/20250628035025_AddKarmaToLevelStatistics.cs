using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250628035025_AddKarmaToLevelStatistics")]
    public partial class AddKarmaToLevelStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Karma",
                table: "GameLevelStatistics",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Karma",
                table: "GameLevelStatistics");
        }
    }
}
