using Refresh.Database.Models.Levels;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers.Migrations;

public class CalculateScoreRanksMigration : MigrationJob<GameLevel>
{
    protected override int BatchCount => 1000;

    protected override IQueryable<GameLevel> SortAndFilter(IQueryable<GameLevel> query)
    {
        return query.OrderBy(l => l.LevelId);
    }

    protected override void Migrate(WorkContext context, GameLevel[] batch)
    {
        foreach (GameLevel level in batch)
        {
            context.Database.RecalculateScoreStatistics(level);
        }
        context.Database.SaveChanges();
    }
}