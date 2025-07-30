using Refresh.Database.Models.Levels;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers.Migrations;

public class BackfillRevisionMigration : MigrationJob<GameLevel>
{
    protected override void Migrate(WorkContext context, GameLevel[] batch)
    {
        foreach (GameLevel level in batch)
        {
            context.Database.CreateRevisionForLevel(level, null);
        }
    }

    protected override IQueryable<GameLevel> SortAndFilter(IQueryable<GameLevel> query)
    {
        return query.OrderBy(l => l.LevelId);
    }
}