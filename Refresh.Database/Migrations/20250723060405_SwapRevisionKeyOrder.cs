using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250723060405_SwapRevisionKeyOrder")]
    /// <inheritdoc />
    public partial class SwapRevisionKeyOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GameLevelRevisions",
                table: "GameLevelRevisions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameLevelRevisions",
                table: "GameLevelRevisions",
                columns: new[] { "RevisionId", "LevelId" });

            migrationBuilder.CreateIndex(
                name: "IX_GameLevelRevisions_LevelId",
                table: "GameLevelRevisions",
                column: "LevelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GameLevelRevisions",
                table: "GameLevelRevisions");

            migrationBuilder.DropIndex(
                name: "IX_GameLevelRevisions_LevelId",
                table: "GameLevelRevisions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GameLevelRevisions",
                table: "GameLevelRevisions",
                columns: new[] { "LevelId", "RevisionId" });
        }
    }
}
