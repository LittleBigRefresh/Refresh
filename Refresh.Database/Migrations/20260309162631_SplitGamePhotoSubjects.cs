using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20260309162631_SplitGamePhotoSubjects")]
    public partial class SplitGamePhotoSubjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GamePhotoSubjects",
                columns: table => new
                {
                    PhotoId = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    Bounds = table.Column<float[]>(type: "real[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePhotoSubjects", x => new { x.PhotoId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_GamePhotoSubjects_GamePhotos_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "GamePhotos",
                        principalColumn: "PhotoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GamePhotoSubjects_GameUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GamePhotoSubjects_UserId",
                table: "GamePhotoSubjects",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GamePhotoSubjects");
        }
    }
}
