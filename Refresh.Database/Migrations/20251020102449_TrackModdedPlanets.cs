using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20251020102449_TrackModdedPlanets")]
    public partial class TrackModdedPlanets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AreBetaPlanetsModded",
                table: "GameUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AreLbp2PlanetsModded",
                table: "GameUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AreLbp3PlanetsModded",
                table: "GameUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AreVitaPlanetsModded",
                table: "GameUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowModdedPlanets",
                table: "GameUsers",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AreBetaPlanetsModded",
                table: "GameUsers");

            migrationBuilder.DropColumn(
                name: "AreLbp2PlanetsModded",
                table: "GameUsers");

            migrationBuilder.DropColumn(
                name: "AreLbp3PlanetsModded",
                table: "GameUsers");

            migrationBuilder.DropColumn(
                name: "AreVitaPlanetsModded",
                table: "GameUsers");

            migrationBuilder.DropColumn(
                name: "ShowModdedPlanets",
                table: "GameUsers");
        }
    }
}
