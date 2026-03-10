using Refresh.Database.Models.Photos;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers.Migrations;

public class MoveSubjectsOutOfGamePhotosMigration : MigrationJob<GamePhoto>
{
    protected override IQueryable<GamePhoto> SortAndFilter(IQueryable<GamePhoto> query)
    {
        return query.OrderBy(p => p.PhotoId);
    }

    protected override void Migrate(WorkContext context, GamePhoto[] batch)
    {
        foreach (GamePhoto photo in batch)
        {
            context.Database.MigratePhotoSubjects(photo, false);
        }

        context.Database.SaveChanges();
    }
}