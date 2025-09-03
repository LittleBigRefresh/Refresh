using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250902151307_AddReviewLabelList")]
    public partial class AddReviewLabelList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Labels",
                table: "GameReviews",
                newName: "LabelsString");

            migrationBuilder.AddColumn<byte[]>(
                name: "Labels",
                table: "GameReviews",
                type: "smallint[]",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Labels",
                table: "GameReviews");
            
            migrationBuilder.RenameColumn(
                name: "LabelsString",
                table: "GameReviews",
                newName: "Labels");
        }
    }
}
