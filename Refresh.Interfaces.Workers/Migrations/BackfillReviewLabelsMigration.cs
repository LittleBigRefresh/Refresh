using Refresh.Database.Models.Comments; 
using Refresh.Workers;

namespace Refresh.Interfaces.Workers.Migrations;

public class BackfillReviewLabelsMigration : MigrationJob<GameReview>
{
    protected override void Migrate(WorkContext context, GameReview[] batch)
    {
        context.Database.MigrateReviewLabels(batch);
    }

    protected override IQueryable<GameReview> SortAndFilter(IQueryable<GameReview> query)
    {
        return query.OrderBy(l => l.ReviewId);
    }
}