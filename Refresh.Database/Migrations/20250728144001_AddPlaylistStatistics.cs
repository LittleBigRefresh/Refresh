using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250728144001_AddPlaylistStatistics")]
    public partial class AddPlaylistStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FavouritePlaylistCount",
                table: "GameUserStatistics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlaylistCount",
                table: "GameUserStatistics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatisticsPlaylistId",
                table: "GamePlaylists",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentPlaylistCount",
                table: "GameLevelStatistics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GamePlaylistStatistics",
                columns: table => new
                {
                    PlaylistId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecalculateAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    FavouriteCount = table.Column<int>(type: "integer", nullable: false),
                    ParentPlaylistCount = table.Column<int>(type: "integer", nullable: false),
                    LevelCount = table.Column<int>(type: "integer", nullable: false),
                    SubPlaylistCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePlaylistStatistics", x => x.PlaylistId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GamePlaylists_StatisticsPlaylistId",
                table: "GamePlaylists",
                column: "StatisticsPlaylistId");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePlaylists_GamePlaylistStatistics_StatisticsPlaylistId",
                table: "GamePlaylists",
                column: "StatisticsPlaylistId",
                principalTable: "GamePlaylistStatistics",
                principalColumn: "PlaylistId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamePlaylists_GamePlaylistStatistics_StatisticsPlaylistId",
                table: "GamePlaylists");

            migrationBuilder.DropTable(
                name: "GamePlaylistStatistics");

            migrationBuilder.DropIndex(
                name: "IX_GamePlaylists_StatisticsPlaylistId",
                table: "GamePlaylists");

            migrationBuilder.DropColumn(
                name: "FavouritePlaylistCount",
                table: "GameUserStatistics");

            migrationBuilder.DropColumn(
                name: "PlaylistCount",
                table: "GameUserStatistics");

            migrationBuilder.DropColumn(
                name: "StatisticsPlaylistId",
                table: "GamePlaylists");

            migrationBuilder.DropColumn(
                name: "ParentPlaylistCount",
                table: "GameLevelStatistics");
        }
    }
}
