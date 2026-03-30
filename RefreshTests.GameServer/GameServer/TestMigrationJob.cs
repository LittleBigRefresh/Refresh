using Refresh.Database.Models.Levels;
using Refresh.Workers;

namespace RefreshTests.GameServer.GameServer;

public class TestMigrationJob : MigrationJob<GameLevel>
{
    protected override int Migrate(WorkContext context, GameLevel[] batch)
    {
        foreach (GameLevel level in batch)
        {
            level.Title += " test";
        }

        return batch.Length;
    }

    protected override IQueryable<GameLevel> SortAndFilter(IQueryable<GameLevel> query)
    {
        return query.OrderBy(l => l.LevelId);
    }
}