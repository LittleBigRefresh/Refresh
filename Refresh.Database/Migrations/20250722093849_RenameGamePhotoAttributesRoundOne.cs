using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250722093849_RenameGamePhotoAttributesRoundOne")]
    public partial class RenameGamePhotoAttributesRoundOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameUsers_PublisherUserId",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameUsers_Subject1UserUserId",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameUsers_Subject2UserUserId",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameUsers_Subject3UserUserId",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameUsers_Subject4UserUserId",
                table: "GamePhotos");

            migrationBuilder.RenameColumn(
                name: "Subject4UserUserId",
                table: "GamePhotos",
                newName: "Subject4UserId");

            migrationBuilder.RenameColumn(
                name: "Subject3UserUserId",
                table: "GamePhotos",
                newName: "Subject3UserId");

            migrationBuilder.RenameColumn(
                name: "Subject2UserUserId",
                table: "GamePhotos",
                newName: "Subject2UserId");

            migrationBuilder.RenameColumn(
                name: "Subject1UserUserId",
                table: "GamePhotos",
                newName: "Subject1UserId");

            migrationBuilder.RenameColumn(
                name: "PublisherUserId",
                table: "GamePhotos",
                newName: "PublisherId");

            migrationBuilder.RenameColumn(
                name: "LevelName",
                table: "GamePhotos",
                newName: "OriginalLevelName");

            migrationBuilder.RenameColumn(
                name: "LevelId",
                table: "GamePhotos",
                newName: "OriginalLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_GamePhotos_Subject4UserUserId",
                table: "GamePhotos",
                newName: "IX_GamePhotos_Subject4UserId");

            migrationBuilder.RenameIndex(
                name: "IX_GamePhotos_Subject3UserUserId",
                table: "GamePhotos",
                newName: "IX_GamePhotos_Subject3UserId");

            migrationBuilder.RenameIndex(
                name: "IX_GamePhotos_Subject2UserUserId",
                table: "GamePhotos",
                newName: "IX_GamePhotos_Subject2UserId");

            migrationBuilder.RenameIndex(
                name: "IX_GamePhotos_Subject1UserUserId",
                table: "GamePhotos",
                newName: "IX_GamePhotos_Subject1UserId");

            migrationBuilder.RenameIndex(
                name: "IX_GamePhotos_PublisherUserId",
                table: "GamePhotos",
                newName: "IX_GamePhotos_PublisherId");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameUsers_PublisherId",
                table: "GamePhotos",
                column: "PublisherId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameUsers_Subject1UserId",
                table: "GamePhotos",
                column: "Subject1UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameUsers_Subject2UserId",
                table: "GamePhotos",
                column: "Subject2UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameUsers_Subject3UserId",
                table: "GamePhotos",
                column: "Subject3UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameUsers_Subject4UserId",
                table: "GamePhotos",
                column: "Subject4UserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameUsers_PublisherId",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameUsers_Subject1UserId",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameUsers_Subject2UserId",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameUsers_Subject3UserId",
                table: "GamePhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_GamePhotos_GameUsers_Subject4UserId",
                table: "GamePhotos");

            migrationBuilder.RenameColumn(
                name: "Subject4UserId",
                table: "GamePhotos",
                newName: "Subject4UserUserId");

            migrationBuilder.RenameColumn(
                name: "Subject3UserId",
                table: "GamePhotos",
                newName: "Subject3UserUserId");

            migrationBuilder.RenameColumn(
                name: "Subject2UserId",
                table: "GamePhotos",
                newName: "Subject2UserUserId");

            migrationBuilder.RenameColumn(
                name: "Subject1UserId",
                table: "GamePhotos",
                newName: "Subject1UserUserId");

            migrationBuilder.RenameColumn(
                name: "PublisherId",
                table: "GamePhotos",
                newName: "PublisherUserId");

            migrationBuilder.RenameColumn(
                name: "OriginalLevelName",
                table: "GamePhotos",
                newName: "LevelName");

            migrationBuilder.RenameColumn(
                name: "OriginalLevelId",
                table: "GamePhotos",
                newName: "LevelId");

            migrationBuilder.RenameIndex(
                name: "IX_GamePhotos_Subject4UserId",
                table: "GamePhotos",
                newName: "IX_GamePhotos_Subject4UserUserId");

            migrationBuilder.RenameIndex(
                name: "IX_GamePhotos_Subject3UserId",
                table: "GamePhotos",
                newName: "IX_GamePhotos_Subject3UserUserId");

            migrationBuilder.RenameIndex(
                name: "IX_GamePhotos_Subject2UserId",
                table: "GamePhotos",
                newName: "IX_GamePhotos_Subject2UserUserId");

            migrationBuilder.RenameIndex(
                name: "IX_GamePhotos_Subject1UserId",
                table: "GamePhotos",
                newName: "IX_GamePhotos_Subject1UserUserId");

            migrationBuilder.RenameIndex(
                name: "IX_GamePhotos_PublisherId",
                table: "GamePhotos",
                newName: "IX_GamePhotos_PublisherUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameUsers_PublisherUserId",
                table: "GamePhotos",
                column: "PublisherUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameUsers_Subject1UserUserId",
                table: "GamePhotos",
                column: "Subject1UserUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameUsers_Subject2UserUserId",
                table: "GamePhotos",
                column: "Subject2UserUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameUsers_Subject3UserUserId",
                table: "GamePhotos",
                column: "Subject3UserUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePhotos_GameUsers_Subject4UserUserId",
                table: "GamePhotos",
                column: "Subject4UserUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");
        }
    }
}
