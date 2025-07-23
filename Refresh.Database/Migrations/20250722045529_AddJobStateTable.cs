using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250722045529_AddJobStateTable")]
    /// <inheritdoc />
    public partial class AddJobStateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobStates",
                columns: table => new
                {
                    JobId = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobStates", x => x.JobId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobStates");
        }
    }
}
