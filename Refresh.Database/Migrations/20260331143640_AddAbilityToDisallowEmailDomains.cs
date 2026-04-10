using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20260331143640_AddAbilityToDisallowEmailDomains")]
    public partial class AddAbilityToDisallowEmailDomains : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "DisallowedEmails", newName: "DisallowedEmailAddresses");
            migrationBuilder.DropPrimaryKey(name: "PK_DisallowedEmails", table: "DisallowedEmailAddresses");
            migrationBuilder.RenameColumn(name: "Email", table: "DisallowedEmailAddresses", newName: "Address");
            migrationBuilder.AddPrimaryKey(name: "PK_DisallowedEmailAddresses", table: "DisallowedEmailAddresses", column: "Address");

            migrationBuilder.CreateTable(
                name: "DisallowedEmailDomains",
                columns: table => new
                {
                    Domain = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisallowedEmailDomains", x => x.Domain);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DisallowedEmailDomains");

            migrationBuilder.RenameTable(name: "DisallowedEmailAddresses", newName: "DisallowedEmails");
            migrationBuilder.DropPrimaryKey(name: "PK_DisallowedEmailAddresses", table: "DisallowedEmails");
            migrationBuilder.RenameColumn(name: "Address", table: "DisallowedEmails", newName: "Email");
            migrationBuilder.AddPrimaryKey(name: "PK_DisallowedEmails", table: "DisallowedEmails", column: "Email");
        }
    }
}
