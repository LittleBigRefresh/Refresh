using Refresh.Database.Models.Photos;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers.Migrations;

public class MoveSubjectsOutOfGamePhotosMigration : MigrationJob<GamePhoto>
{
    protected override IQueryable<GamePhoto> SortAndFilter(IQueryable<GamePhoto> query)
    {
        return query
            .Where(p => p.Subjects.Count == 0)
            .OrderBy(p => p.PhotoId);
    }

    protected override void Migrate(WorkContext context, GamePhoto[] batch)
    {
        foreach (GamePhoto photo in batch)
        {
            // Extra check to be sure
            if (context.Database.GetTotalSubjectsInPhoto(photo) > 0)
                continue;

            #pragma warning disable CS0618 // obsoletion

            // If DisplayName is not null, there is a subject in that spot
            if (photo.Subject1DisplayName != null)
            {
                context.Database.AddSubjectForPhoto(photo, 1, photo.Subject1DisplayName, photo.Subject1User, photo.Subject1Bounds, false);
            }

            if (photo.Subject2DisplayName != null)
            {
                context.Database.AddSubjectForPhoto(photo, 2, photo.Subject2DisplayName, photo.Subject2User, photo.Subject2Bounds, false);
            }

            if (photo.Subject3DisplayName != null)
            {
                context.Database.AddSubjectForPhoto(photo, 3, photo.Subject3DisplayName, photo.Subject3User, photo.Subject3Bounds, false);
            }

            if (photo.Subject4DisplayName != null)
            {
                context.Database.AddSubjectForPhoto(photo, 4, photo.Subject4DisplayName, photo.Subject4User, photo.Subject4Bounds, false);
            }

            #pragma warning restore CS0618
        }

        context.Database.SaveChanges();
    }
}