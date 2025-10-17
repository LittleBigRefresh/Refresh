using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20251008175307_AnnihilateScoreType7")]
    public partial class AnnihilateScoreType7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Just set all type 7 scores' type to 1 since we haven't tracked more than 1 user 
            // in the player list before anyway
            migrationBuilder.Sql("UPDATE \"GameScores\" SET \"ScoreType\" = 1 WHERE \"ScoreType\" = 7");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Do nothing
        }
    }
}
