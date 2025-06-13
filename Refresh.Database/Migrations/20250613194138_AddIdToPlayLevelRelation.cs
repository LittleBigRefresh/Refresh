using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250613194138_AddIdToPlayLevelRelation")]
    public partial class AddIdToPlayLevelRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // we make changes here that break existing plays
            migrationBuilder.Sql("DELETE FROM \"PlayLevelRelations\";");
            
            migrationBuilder.DropPrimaryKey(
                name: "PK_PlayLevelRelations",
                table: "PlayLevelRelations");

            migrationBuilder.AddColumn<string>(
                name: "PlayId",
                table: "PlayLevelRelations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlayLevelRelations",
                table: "PlayLevelRelations",
                column: "PlayId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayLevelRelations_LevelId",
                table: "PlayLevelRelations",
                column: "LevelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PlayLevelRelations",
                table: "PlayLevelRelations");

            migrationBuilder.DropIndex(
                name: "IX_PlayLevelRelations_LevelId",
                table: "PlayLevelRelations");

            migrationBuilder.DropColumn(
                name: "PlayId",
                table: "PlayLevelRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlayLevelRelations",
                table: "PlayLevelRelations",
                columns: new[] { "LevelId", "UserId" });
        }
    }
}
