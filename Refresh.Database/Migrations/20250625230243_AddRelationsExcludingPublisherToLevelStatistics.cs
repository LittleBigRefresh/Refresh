using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250625230243_AddRelationsExcludingPublisherToLevelStatistics")]
    /// <inheritdoc />
    public partial class AddRelationsExcludingPublisherToLevelStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BooCountExcludingPublisher",
                table: "GameLevelStatistics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FavouriteCountExcludingPublisher",
                table: "GameLevelStatistics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NeutralCountExcludingPublisher",
                table: "GameLevelStatistics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UniquePlayCountExcludingPublisher",
                table: "GameLevelStatistics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "YayCountExcludingPublisher",
                table: "GameLevelStatistics",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BooCountExcludingPublisher",
                table: "GameLevelStatistics");

            migrationBuilder.DropColumn(
                name: "FavouriteCountExcludingPublisher",
                table: "GameLevelStatistics");

            migrationBuilder.DropColumn(
                name: "NeutralCountExcludingPublisher",
                table: "GameLevelStatistics");

            migrationBuilder.DropColumn(
                name: "UniquePlayCountExcludingPublisher",
                table: "GameLevelStatistics");

            migrationBuilder.DropColumn(
                name: "YayCountExcludingPublisher",
                table: "GameLevelStatistics");
        }
    }
}
