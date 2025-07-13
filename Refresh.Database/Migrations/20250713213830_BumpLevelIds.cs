using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Refresh.Database.Migrations
{
    /// <inheritdoc />
    [Migration("20250713213830_BumpLevelIds")]
    [DbContext(typeof(GameDatabaseContext))]
    public partial class BumpLevelIds : Migration
    {
        private void AddForeignKey(MigrationBuilder migration, string name, string table, string column = "LevelId")
        {
            migration.Sql($"UPDATE \"{table}\" SET \"{column}\" = \"{column}\" + 200000000");
            migration.AddForeignKey(name, table, column, "GameLevels", principalColumn: "LevelId", onDelete: ReferentialAction.Cascade);
        }
        
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migration)
        {
            migration.Sql("SELECT setval('\"GameLevels_LevelId_seq\"', nextval('\"GameLevels_LevelId_seq\"') + 200000000);");

            migration.DropForeignKey("FK_FavouriteLevelRelations_GameLevels_LevelId", "FavouriteLevelRelations");
            migration.DropForeignKey("FK_GameChallenges_GameLevels_LevelId", "GameChallenges");
            migration.DropForeignKey("FK_GameContests_GameLevels_TemplateLevelLevelId", "GameContests");
            migration.DropForeignKey("FK_GameLevelComments_GameLevels_LevelId", "GameLevelComments");
            migration.DropForeignKey("FK_GamePhotos_GameLevels_LevelIdKey", "GamePhotos");
            migration.DropForeignKey("FK_GameReviews_GameLevels_LevelId", "GameReviews");
            migration.DropForeignKey("FK_GameSkillRewards_GameLevels_LevelId", "GameSkillRewards");
            migration.DropForeignKey("FK_GameSubmittedScores_GameLevels_LevelId", "GameScores");
            migration.DropForeignKey("FK_LevelPlaylistRelations_GameLevels_LevelId", "LevelPlaylistRelations");
            migration.DropForeignKey("FK_PlayLevelRelations_GameLevels_LevelId", "PlayLevelRelations");
            migration.DropForeignKey("FK_QueueLevelRelations_GameLevels_LevelId", "QueueLevelRelations");
            migration.DropForeignKey("FK_RateLevelRelations_GameLevels_LevelId", "RateLevelRelations");
            migration.DropForeignKey("FK_TagLevelRelations_GameLevels_LevelId", "TagLevelRelations");
            migration.DropForeignKey("FK_UniquePlayLevelRelations_GameLevels_LevelId", "UniquePlayLevelRelations");
            
            migration.Sql("UPDATE \"GameLevels\" SET \"LevelId\" = \"LevelId\" + 200000000");
            
            AddForeignKey(migration, "FK_FavouriteLevelRelations_GameLevels_LevelId", "FavouriteLevelRelations");
            AddForeignKey(migration, "FK_GameChallenges_GameLevels_LevelId", "GameChallenges");
            AddForeignKey(migration, "FK_GameContests_GameLevels_TemplateLevelLevelId", "GameContests", "TemplateLevelLevelId");
            AddForeignKey(migration, "FK_GameLevelComments_GameLevels_LevelId", "GameLevelComments");
            AddForeignKey(migration, "FK_GamePhotos_GameLevels_LevelIdKey", "GamePhotos", "LevelIdKey");
            AddForeignKey(migration, "FK_GameReviews_GameLevels_LevelId", "GameReviews");
            AddForeignKey(migration, "FK_GameSkillRewards_GameLevels_LevelId", "GameSkillRewards");
            AddForeignKey(migration, "FK_GameSubmittedScores_GameLevels_LevelId", "GameScores");
            AddForeignKey(migration, "FK_LevelPlaylistRelations_GameLevels_LevelId", "LevelPlaylistRelations");
            AddForeignKey(migration, "FK_PlayLevelRelations_GameLevels_LevelId", "PlayLevelRelations");
            AddForeignKey(migration, "FK_QueueLevelRelations_GameLevels_LevelId", "QueueLevelRelations");
            AddForeignKey(migration, "FK_RateLevelRelations_GameLevels_LevelId", "RateLevelRelations");
            AddForeignKey(migration, "FK_TagLevelRelations_GameLevels_LevelId", "TagLevelRelations");
            AddForeignKey(migration, "FK_UniquePlayLevelRelations_GameLevels_LevelId", "UniquePlayLevelRelations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migration)
        {
            migration.Sql("SELECT setval('GameLevels_LevelId_seq', nextval('GameLevels_LevelId_seq') - 200000000);");
            migration.Sql("UPDATE \"GameLevels\" SET \"LevelId\" = \"LevelId\" - 200000000");
        }
    }
}
