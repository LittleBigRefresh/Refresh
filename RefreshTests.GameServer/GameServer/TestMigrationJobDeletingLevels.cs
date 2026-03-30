using Refresh.Database.Models.Levels;
using Refresh.Workers;

namespace RefreshTests.GameServer.GameServer;

public class TestMigrationJobDeletingLevels : MigrationJob<GameLevel>
{
    protected override int BatchCount => 6;

    // Deletes every second level
    protected override int Migrate(WorkContext context, GameLevel[] batch)
    {
        int entitiesLeft = batch.Length;
        int index = 0;
        foreach (GameLevel level in batch)
        {
            if (index % 2 == 1)
            {
                context.Database.DeleteLevel(level);
                entitiesLeft--;
            }
            else
            {
                level.Title += " test";
            }

            index++;
        }

        return entitiesLeft;
    }

    protected override IQueryable<GameLevel> SortAndFilter(IQueryable<GameLevel> query)
    {
        return query.OrderBy(l => l.LevelId);
    }
}