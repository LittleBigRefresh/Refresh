using Refresh.Database.Models.Levels;
using Refresh.Workers;

namespace RefreshTests.GameServer.GameServer;

public class TestMigrationJob : MigrationJob<GameLevel>
{
    protected override void Migrate(WorkContext context, GameLevel[] batch)
    {
        foreach (GameLevel level in batch)
        {
            level.Title += " test";
        }
    }

    protected override IQueryable<GameLevel> SortAndFilter(IQueryable<GameLevel> query)
    {
        return query.OrderBy(l => l.LevelId);
    }
}