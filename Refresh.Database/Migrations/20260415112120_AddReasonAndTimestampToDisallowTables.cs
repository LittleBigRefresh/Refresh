using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20260415112120_AddReasonAndTimestampToDisallowTables")]
    public partial class AddReasonAndTimestampToDisallowTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DisallowedAt",
                table: "DisallowedUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "DisallowedUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DisallowedAt",
                table: "DisallowedEmailDomains",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "DisallowedEmailDomains",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DisallowedAt",
                table: "DisallowedEmailAddresses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "DisallowedEmailAddresses",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisallowedAt",
                table: "DisallowedUsers");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "DisallowedUsers");

            migrationBuilder.DropColumn(
                name: "DisallowedAt",
                table: "DisallowedEmailDomains");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "DisallowedEmailDomains");

            migrationBuilder.DropColumn(
                name: "DisallowedAt",
                table: "DisallowedEmailAddresses");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "DisallowedEmailAddresses");
        }
    }
}
