using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20261017093718_MakeReviewedByUserIdNotRequiredAgain")]
    public partial class MakeReviewedByUserIdNotRequiredAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameGriefReports_GameUsers_ReviewedByUserId",
                table: "GameGriefReports");

            migrationBuilder.AlterColumn<string>(
                name: "ReviewedByUserId",
                table: "GameGriefReports",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_GameGriefReports_GameUsers_ReviewedByUserId",
                table: "GameGriefReports",
                column: "ReviewedByUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameGriefReports_GameUsers_ReviewedByUserId",
                table: "GameGriefReports");

            migrationBuilder.AlterColumn<string>(
                name: "ReviewedByUserId",
                table: "GameGriefReports",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GameGriefReports_GameUsers_ReviewedByUserId",
                table: "GameGriefReports",
                column: "ReviewedByUserId",
                principalTable: "GameUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
