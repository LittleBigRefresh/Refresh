using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250713222256_BumpLevelIdsInRecentActivity")]
    public partial class BumpLevelIdsInRecentActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migration)
        {
            migration.Sql("UPDATE \"Events\" SET \"StoredSequentialId\" = \"StoredSequentialId\" + 200000000 WHERE \"StoredDataType\" = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migration)
        {
            migration.Sql("UPDATE \"Events\" SET \"StoredSequentialId\" = \"StoredSequentialId\" - 200000000 WHERE \"StoredDataType\" = 1");
        }
    }
}
