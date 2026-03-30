using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Levels;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers.Migrations;

public class BackfillReviewLabelsMigration : MigrationJob<GameReview>
{
    protected override void Migrate(WorkContext context, GameReview[] batch)
    {
        foreach (GameReview review in batch)
        {
#pragma warning disable CS0618 // LabelsString is obsolete          
            if (string.IsNullOrWhiteSpace(review.LabelsString)) continue;

            context.Database.Update(review);
            review.Labels = LabelExtensions.FromLbpCommaList(review.LabelsString).ToList();
#pragma warning restore CS0618
        }

        context.Database.SaveChanges();
    }

    protected override IQueryable<GameReview> SortAndFilter(IQueryable<GameReview> query)
    {
        return query.OrderBy(l => l.ReviewId);
    }
}