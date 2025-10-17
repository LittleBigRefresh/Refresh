using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20251012133948_TrackReviewUpdateDates")]
    public partial class TrackReviewUpdateDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "GameReviews",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("UPDATE \"GameReviews\" SET \"UpdatedAt\" = \"PostedAt\"");

            // PostedAt is NOT NULL, so there shouldn't be any issues doing this
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "GameReviews",
                nullable: false,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "GameReviews");
        }
    }
}
