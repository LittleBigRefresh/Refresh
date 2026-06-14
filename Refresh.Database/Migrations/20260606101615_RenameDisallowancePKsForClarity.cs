using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20260606101615_RenameDisallowancePKsForClarity")]
    public partial class RenameDisallowancePKsForClarity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Username",
                table: "DisallowedUsers",
                newName: "UsernameLower");

            migrationBuilder.RenameColumn(
                name: "Domain",
                table: "DisallowedEmailDomains",
                newName: "DomainLower");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "DisallowedEmailAddresses",
                newName: "AddressLower");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UsernameLower",
                table: "DisallowedUsers",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "DomainLower",
                table: "DisallowedEmailDomains",
                newName: "Domain");

            migrationBuilder.RenameColumn(
                name: "AddressLower",
                table: "DisallowedEmailAddresses",
                newName: "Address");
        }
    }
}
