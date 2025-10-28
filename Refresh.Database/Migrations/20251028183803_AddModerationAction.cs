using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(GameDatabaseContext))]
    [Migration("20251028183803_AddModerationAction")]
    public partial class AddModerationAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModerationActions",
                columns: table => new
                {
                    ActionId = table.Column<string>(type: "text", nullable: false),
                    ActionType = table.Column<byte>(type: "smallint", nullable: false),
                    ModeratedObjectType = table.Column<byte>(type: "smallint", nullable: false),
                    ModeratedObjectId = table.Column<string>(type: "text", nullable: false),
                    ActorId = table.Column<string>(type: "text", nullable: false),
                    InvolvedUserId = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationActions", x => x.ActionId);
                    table.ForeignKey(
                        name: "FK_ModerationActions_GameUsers_ActorId",
                        column: x => x.ActorId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModerationActions_GameUsers_InvolvedUserId",
                        column: x => x.InvolvedUserId,
                        principalTable: "GameUsers",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationActions_ActorId",
                table: "ModerationActions",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationActions_InvolvedUserId",
                table: "ModerationActions",
                column: "InvolvedUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModerationActions");
        }
    }
}
