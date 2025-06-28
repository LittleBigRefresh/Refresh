using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    // [DbContext(typeof(GameDatabaseContext))]
    // [Migration("20250628042918_IndexEventTimestamp")]
    public partial class IndexEventTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE INDEX \"IX_Events_Timestamp\" ON \"Events\" (\"Timestamp\" DESC);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Events_Timestamp",
                table: "Events");
        }
    }
}
