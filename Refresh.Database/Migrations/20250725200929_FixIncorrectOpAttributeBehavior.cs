using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250725200929_FixIncorrectOpAttributeBehavior")]
    /// <inheritdoc />
    public partial class FixIncorrectOpAttributeBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"GameLevels\" SET \"OriginalPublisher\" = null WHERE \"OriginalPublisher\" = '!Unknown'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // no-op, this is part of a bugfix
        }
    }
}
