using Refresh.Database.Models.Levels;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers.Migrations;

public class BackfillLevelAttributesMigration : MigrationJob<GameLevel>
{
    protected override IQueryable<GameLevel> SortAndFilter(IQueryable<GameLevel> query)
    {
        return query
            .Where(l => l.StoryId == 0)
            .OrderBy(l => l.LevelId);
    }

    protected override void Migrate(WorkContext context, GameLevel[] batch)
    {
        foreach (GameLevel level in batch)
        {
            context.Database.ApplyLevelMetadataFromAttributes(level);
        }

        context.Database.SaveChanges();
    }
}