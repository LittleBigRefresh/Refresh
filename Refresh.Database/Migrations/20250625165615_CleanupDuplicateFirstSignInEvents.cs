using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    public partial class CleanupDuplicateFirstSignInEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM "Events" T1
                  USING "Events" T2
                WHERE T1."_EventType" = 127
                  AND T1."Timestamp" > T2."Timestamp"
                  AND T1."UserId" = T2."UserId";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
