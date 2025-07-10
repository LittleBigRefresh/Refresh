using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250705152755_FixPlaylistCreation")]
    public partial class FixPlaylistCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GamePlaylists_PublisherId",
                table: "GamePlaylists");

            migrationBuilder.CreateIndex(
                name: "IX_GamePlaylists_PublisherId",
                table: "GamePlaylists",
                column: "PublisherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GamePlaylists_PublisherId",
                table: "GamePlaylists");

            migrationBuilder.CreateIndex(
                name: "IX_GamePlaylists_PublisherId",
                table: "GamePlaylists",
                column: "PublisherId",
                unique: true);
        }
    }
}
