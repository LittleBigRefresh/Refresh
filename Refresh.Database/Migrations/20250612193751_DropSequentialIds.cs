using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250612193751_DropSequentialIds")]
    public partial class DropSequentialIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SequentialId",
                table: "GameReviews");

            migrationBuilder.DropColumn(
                name: "SequentialId",
                table: "GamePlaylists");

            migrationBuilder.DropColumn(
                name: "SequentialId",
                table: "GamePhotos");

            migrationBuilder.DropColumn(
                name: "SequentialId",
                table: "GameLevels");

            migrationBuilder.DropColumn(
                name: "SequentialId",
                table: "GameChallenges");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SequentialId",
                table: "GameReviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SequentialId",
                table: "GamePlaylists",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SequentialId",
                table: "GamePhotos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SequentialId",
                table: "GameLevels",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SequentialId",
                table: "GameChallenges",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
