using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20261013102558_MiscGriefReportQualityUpdates")]
    public partial class MiscGriefReportQualityUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Reports",
                newName: "GameGriefReports");

            // thank you for this typo smh
            migrationBuilder.RenameTable(
                name: "ReportPlayersRelations",
                newName: "ReportPlayerRelations");

            migrationBuilder.AlterColumn<string>(
                name: "PlayerRect",
                table: "ReportPlayersRelations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ReportPlayersRelations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "ReportPlayersRelations",
                type: "text",
                nullable: false,
                defaultValue: "");

            // (Re)init the player number column so we can ensure it only has unique values per report ID,
            // so we can safely use it to replace UserId in the primary key
            // https://stackoverflow.com/questions/65231516/add-and-update-a-column-with-sequential-numbering-based-on-grouping
            migrationBuilder.Sql
            ("""
                UPDATE rpr SET rpr."PlayerNumber" = newNum
                FROM (
                    SELECT "ReportPlayerRelations".*, row_number()
                    OVER (PARTITION BY "ReportId" ORDER BY "ReportId") newNum
                    FROM "ReportPlayerRelations"
                ) rpr
            """);

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportPlayersRelations",
                table: "ReportPlayersRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportPlayersRelations",
                table: "ReportPlayersRelations",
                columns: new[] { "ReportId", "PlayerNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "GameGriefReports",
                newName: "Reports");
                
            migrationBuilder.RenameTable(
                name: "ReportPlayerRelations",
                newName: "ReportPlayersRelations");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ReportPlayersRelations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PlayerRect",
                table: "ReportPlayersRelations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            // Just remove all with duplicate user IDs. Usually we won't have duplicate IDs anyway,
            // and this should be enough for a rollback. Better than dropping the whole table or something.
            // We can just use PlayerNumber to uniquely identify rows per ReportId, since we've already made sure
            // they're actually unique in the update (which we're coming from if we're downgrading).
            migrationBuilder.Sql
            ("""
                DELETE FROM "ReportPlayersRelations"
                WHERE "PlayerNumber" NOT IN (
                    SELECT "PlayerNumber"
                    FROM "ReportPlayersRelations"
                    PARTITION BY "ReportId"
                    GROUP BY "UserId"
                    ORDER BY "PlayerNumber"
                )
            """);

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportPlayersRelations",
                table: "ReportPlayersRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportPlayersRelations",
                table: "ReportPlayersRelations",
                columns: new[] { "ReportId", "UserId" });
        }
    }
}
