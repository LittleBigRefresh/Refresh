using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Pins;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20251029184346_SeperatePinProgressByPlatform")]
    public partial class SeperatePinProgressByPlatform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // RPCS3 is the safest default value here, as game achievements on RPCS3 are considered the least valuable.
            // Also, the game will sync progress and profile pins after login anyway, so practically no information is lost here.
            migrationBuilder.AddColumn<int>(
                name: "Platform",
                table: "ProfilePinRelations",
                type: "integer",
                nullable: false,
                defaultValue: TokenPlatform.RPCS3); 

            migrationBuilder.AddColumn<int>(
                name: "Platform",
                table: "PinProgressRelations",
                type: "integer",
                nullable: false,
                defaultValue: TokenPlatform.RPCS3);
                
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfilePinRelations",
                table: "ProfilePinRelations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PinProgressRelations",
                table: "PinProgressRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfilePinRelations",
                table: "ProfilePinRelations",
                columns: ["Index", "PublisherId", "Game", "Platform"]);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PinProgressRelations",
                table: "PinProgressRelations",
                columns: ["PinId", "PublisherId", "IsBeta", "Platform"]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Platform",
                table: "ProfilePinRelations");

            migrationBuilder.DropColumn(
                name: "Platform",
                table: "PinProgressRelations");
            
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfilePinRelations",
                table: "ProfilePinRelations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PinProgressRelations",
                table: "PinProgressRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfilePinRelations",
                table: "ProfilePinRelations",
                columns: ["Index", "PublisherId", "Game"]);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PinProgressRelations",
                table: "PinProgressRelations",
                columns: ["PinId", "PublisherId", "IsBeta"]);
        }
    }
}
