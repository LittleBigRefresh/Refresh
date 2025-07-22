using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250722094127_RenameGamePhotoAttributesRoundTwo")]
    public partial class RenameGamePhotoAttributesRoundTwo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameAssets_LargeAssetAssetHash",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameAssets_MediumAssetAssetHash",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameAssets_SmallAssetAssetHash",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameLevels_LevelIdKey",
                table: "GamePhotos");

            migrationBuilder.RenameColumn(
                name: "SmallAssetAssetHash",
                table: "GamePhotos",
                newName: "SmallAssetHash");

            migrationBuilder.RenameColumn(
                name: "MediumAssetAssetHash",
                table: "GamePhotos",
                newName: "MediumAssetHash");

            migrationBuilder.RenameColumn(
                name: "LevelIdKey",
                table: "GamePhotos",
                newName: "LevelId");

            migrationBuilder.RenameColumn(
                name: "LargeAssetAssetHash",
                table: "GamePhotos",
                newName: "LargeAssetHash");

            migrationBuilder.RenameIndex(
                name: "IX_GamePhotos_SmallAssetAssetHash",
                table: "GamePhotos",
                newName: "IX_GamePhotos_SmallAssetHash");

            migrationBuilder.RenameIndex(
                name: "IX_GamePhotos_MediumAssetAssetHash",
                table: "GamePhotos",
                newName: "IX_GamePhotos_MediumAssetHash");

            migrationBuilder.RenameIndex(
                name: "IX_GamePhotos_LevelIdKey",
                table: "GamePhotos",
                newName: "IX_GamePhotos_LevelId");

            migrationBuilder.RenameIndex(
                name: "IX_GamePhotos_LargeAssetAssetHash",
                table: "GamePhotos",
                newName: "IX_GamePhotos_LargeAssetHash");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameAssets_LargeAssetHash",
                table: "GamePhotos",
                column: "LargeAssetHash",
                principalTable: "GameAssets",
                principalColumn: "AssetHash",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameAssets_MediumAssetHash",
                table: "GamePhotos",
                column: "MediumAssetHash",
                principalTable: "GameAssets",
                principalColumn: "AssetHash",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameAssets_SmallAssetHash",
                table: "GamePhotos",
                column: "SmallAssetHash",
                principalTable: "GameAssets",
                principalColumn: "AssetHash",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameLevels_LevelId",
                table: "GamePhotos",
                column: "LevelId",
                principalTable: "GameLevels",
                principalColumn: "LevelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameAssets_LargeAssetHash",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameAssets_MediumAssetHash",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameAssets_SmallAssetHash",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameLevels_LevelId",
                table: "GamePhotos");

            migrationBuilder.RenameColumn(
                name: "SmallAssetHash",
                table: "GamePhotos",
                newName: "SmallAssetAssetHash");

            migrationBuilder.RenameColumn(
                name: "MediumAssetHash",
                table: "GamePhotos",
                newName: "MediumAssetAssetHash");

            migrationBuilder.RenameColumn(
                name: "LevelId",
                table: "GamePhotos",
                newName: "LevelIdKey");

            migrationBuilder.RenameColumn(
                name: "LargeAssetHash",
                table: "GamePhotos",
                newName: "LargeAssetAssetHash");

            migrationBuilder.RenameIndex(
                name: "IX_GamePhotos_SmallAssetHash",
                table: "GamePhotos",
                newName: "IX_GamePhotos_SmallAssetAssetHash");

            migrationBuilder.RenameIndex(
                name: "IX_GamePhotos_MediumAssetHash",
                table: "GamePhotos",
                newName: "IX_GamePhotos_MediumAssetAssetHash");

            migrationBuilder.RenameIndex(
                name: "IX_GamePhotos_LevelId",
                table: "GamePhotos",
                newName: "IX_GamePhotos_LevelIdKey");

            migrationBuilder.RenameIndex(
                name: "IX_GamePhotos_LargeAssetHash",
                table: "GamePhotos",
                newName: "IX_GamePhotos_LargeAssetAssetHash");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameAssets_LargeAssetAssetHash",
                table: "GamePhotos",
                column: "LargeAssetAssetHash",
                principalTable: "GameAssets",
                principalColumn: "AssetHash",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameAssets_MediumAssetAssetHash",
                table: "GamePhotos",
                column: "MediumAssetAssetHash",
                principalTable: "GameAssets",
                principalColumn: "AssetHash",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameAssets_SmallAssetAssetHash",
                table: "GamePhotos",
                column: "SmallAssetAssetHash",
                principalTable: "GameAssets",
                principalColumn: "AssetHash",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameLevels_LevelIdKey",
                table: "GamePhotos",
                column: "LevelIdKey",
                principalTable: "GameLevels",
                principalColumn: "LevelId");
        }
    }
}
