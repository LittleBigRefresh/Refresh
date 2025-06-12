using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250612200425_AddDedicatedKeyToGamePhoto")]
    public partial class AddDedicatedKeyToGamePhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameLevels_LevelId",
                table: "GamePhotos");

            migrationBuilder.DropIndex(
                name: "IX_GamePhotos_LevelId",
                table: "GamePhotos");

            migrationBuilder.AddColumn<int>(
                name: "LevelIdKey",
                table: "GamePhotos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GamePhotos_LevelIdKey",
                table: "GamePhotos",
                column: "LevelIdKey");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameLevels_LevelIdKey",
                table: "GamePhotos",
                column: "LevelIdKey",
                principalTable: "GameLevels",
                principalColumn: "LevelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameLevels_LevelIdKey",
                table: "GamePhotos");

            migrationBuilder.DropIndex(
                name: "IX_GamePhotos_LevelIdKey",
                table: "GamePhotos");

            migrationBuilder.DropColumn(
                name: "LevelIdKey",
                table: "GamePhotos");

            migrationBuilder.CreateIndex(
                name: "IX_GamePhotos_LevelId",
                table: "GamePhotos",
                column: "LevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameLevels_LevelId",
                table: "GamePhotos",
                column: "LevelId",
                principalTable: "GameLevels",
                principalColumn: "LevelId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
