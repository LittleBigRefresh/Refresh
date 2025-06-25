using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20250625194345_AddVisibilityToGameUsers")]
    /// <inheritdoc />
    public partial class AddVisibilityToGameUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migration)
        {
            migration.AddColumn<int>("LevelVisibility", "GameUsers", type: "integer", nullable: false, defaultValue: 0);
            migration.AddColumn<int>("ProfileVisibility", "GameUsers", type: "integer", nullable: false, defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migration)
        {
            migration.DropColumn("LevelVisibility", "GameUsers");
            migration.DropColumn("ProfileVisibility", "GameUsers");
        }
    }
}
