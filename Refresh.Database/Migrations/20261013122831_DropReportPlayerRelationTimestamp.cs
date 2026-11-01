using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20261013122831_DropReportPlayerRelationTimestamp")]
    public partial class DropReportPlayerRelationTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This column had no setters at all, meaning all rows had this set to -infinity.
            // The report uploads also don't actually contain any player-specific timestamps,
            // so this is safe to drop.
            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "ReportPlayerRelations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Timestamp",
                table: "ReportPlayerRelations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        }
    }
}
