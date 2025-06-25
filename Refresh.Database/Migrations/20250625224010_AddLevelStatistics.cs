using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Refresh.Database.Migrations
{
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250625224010_AddLevelStatistics")]
    /// <inheritdoc />
    public partial class AddLevelStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatisticsLevelId",
                table: "GameLevels",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GameLevelStatistics",
                columns: table => new
                {
                    LevelId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FavouriteCount = table.Column<int>(type: "integer", nullable: false),
                    PlayCount = table.Column<int>(type: "integer", nullable: false),
                    UniquePlayCount = table.Column<int>(type: "integer", nullable: false),
                    CompletionCount = table.Column<int>(type: "integer", nullable: false),
                    ReviewCount = table.Column<int>(type: "integer", nullable: false),
                    CommentCount = table.Column<int>(type: "integer", nullable: false),
                    PhotoInLevelCount = table.Column<int>(type: "integer", nullable: false),
                    PhotoByPublisherCount = table.Column<int>(type: "integer", nullable: false),
                    YayCount = table.Column<int>(type: "integer", nullable: false),
                    BooCount = table.Column<int>(type: "integer", nullable: false),
                    NeutralCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameLevelStatistics", x => x.LevelId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameLevels_StatisticsLevelId",
                table: "GameLevels",
                column: "StatisticsLevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameLevels_GameLevelStatistics_StatisticsLevelId",
                table: "GameLevels",
                column: "StatisticsLevelId",
                principalTable: "GameLevelStatistics",
                principalColumn: "LevelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameLevels_GameLevelStatistics_StatisticsLevelId",
                table: "GameLevels");

            migrationBuilder.DropTable(
                name: "GameLevelStatistics");

            migrationBuilder.DropIndex(
                name: "IX_GameLevels_StatisticsLevelId",
                table: "GameLevels");

            migrationBuilder.DropColumn(
                name: "StatisticsLevelId",
                table: "GameLevels");
        }
    }
}
