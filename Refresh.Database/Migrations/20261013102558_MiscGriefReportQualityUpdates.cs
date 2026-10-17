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
            migrationBuilder.AlterColumn<string>(
                name: "PlayerRect",
                table: "ReportPlayersRelations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "ReportPlayersRelations",
                type: "text",
                nullable: false,
                defaultValue: "");

            // ReportPlayerRelations PK changes (since we're altering UserId, which is part of PK at this point)

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportPlayersRelations",
                table: "ReportPlayersRelations");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ReportPlayersRelations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            // (Re)init the player number column so we can ensure it only has unique values per report ID,
            // so we can safely use it to replace UserId in the primary key
            // https://stackoverflow.com/questions/65231516/add-and-update-a-column-with-sequential-numbering-based-on-grouping
            migrationBuilder.Sql
            ("""
                UPDATE "ReportPlayersRelations" SET "PlayerNumber" = u.newNum
                FROM (
                    SELECT *, row_number() OVER (
                        PARTITION BY "ReportId"
                        ORDER BY "ReportId"
                    ) AS newNum
                    FROM "ReportPlayersRelations"
                ) AS u
                WHERE "ReportPlayersRelations"."ReportId" = u."ReportId"
                AND "ReportPlayersRelations"."UserId" = u."UserId"
            """);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportPlayerRelations",
                table: "ReportPlayersRelations",
                columns: new[] { "ReportId", "PlayerNumber" });

            // Doing this in the middle caused random issues with the following DB commands,
            // so rename the tables after everything.

            migrationBuilder.RenameTable(
                name: "Reports",
                newName: "GameGriefReports");

            // thank you for this typo smh
            migrationBuilder.RenameTable(
                name: "ReportPlayersRelations",
                newName: "ReportPlayerRelations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ReportPlayerRelations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PlayerRect",
                table: "ReportPlayerRelations",
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
                DELETE FROM "ReportPlayerRelations"
                WHERE "PlayerNumber" NOT IN (
                    SELECT "PlayerNumber"
                    FROM "ReportPlayerRelations"
                    PARTITION BY "ReportId"
                    GROUP BY "UserId"
                    ORDER BY "PlayerNumber"
                )
            """);

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportPlayerRelations",
                table: "ReportPlayerRelations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportPlayersRelations",
                table: "ReportPlayerRelations",
                columns: new[] { "ReportId", "UserId" });

            migrationBuilder.RenameTable(
                name: "GameGriefReports",
                newName: "Reports");

            migrationBuilder.RenameTable(
                name: "ReportPlayerRelations",
                newName: "ReportPlayersRelations");
        }
    }
}
