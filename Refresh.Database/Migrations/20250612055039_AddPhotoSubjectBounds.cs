using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250612055039_AddPhotoSubjectBounds")]
    public partial class AddPhotoSubjectBounds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<float>>(
                name: "Subject1Bounds",
                table: "GamePhotos",
                type: "real[]",
                nullable: false);

            migrationBuilder.AddColumn<List<float>>(
                name: "Subject2Bounds",
                table: "GamePhotos",
                type: "real[]",
                nullable: false);

            migrationBuilder.AddColumn<List<float>>(
                name: "Subject3Bounds",
                table: "GamePhotos",
                type: "real[]",
                nullable: false);

            migrationBuilder.AddColumn<List<float>>(
                name: "Subject4Bounds",
                table: "GamePhotos",
                type: "real[]",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Subject1Bounds",
                table: "GamePhotos");

            migrationBuilder.DropColumn(
                name: "Subject2Bounds",
                table: "GamePhotos");

            migrationBuilder.DropColumn(
                name: "Subject3Bounds",
                table: "GamePhotos");

            migrationBuilder.DropColumn(
                name: "Subject4Bounds",
                table: "GamePhotos");
        }
    }
}
