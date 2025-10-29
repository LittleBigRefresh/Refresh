using MongoDB.Bson;
using Refresh.Common.Time;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Pins;
using Refresh.Database.Models.Relations;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers.Migrations;

public class CorrectWebsitePinProgressPlatform : MigrationJob<PinProgressRelation>
{
    private List<long> websitePinIds =
    [
        (long)ServerPins.HeartPlayerOnWebsite,
        (long)ServerPins.QueueLevelOnWebsite,
        (long)ServerPins.SignIntoWebsite,
    ];

    protected override IQueryable<PinProgressRelation> SortAndFilter(IQueryable<PinProgressRelation> query)
    {
        return query
            .Where(p => this.websitePinIds.Contains(p.PinId))
            .OrderBy(p => p.PinId);
    }

    protected override void Migrate(WorkContext context, PinProgressRelation[] batch)
    {
        foreach (long pinId in this.websitePinIds)
        {
            IEnumerable<IGrouping<ObjectId, PinProgressRelation>> pinsByUser = batch
                .Where(r => r.PinId == pinId)
                .GroupBy(r => r.PublisherId);
            
            foreach (IEnumerable<PinProgressRelation> group in pinsByUser)
            {
                if (!group.Any()) continue;

                // Find and migrate one progress
                PinProgressRelation relationToMigrate = group.MaxBy(r => r.Progress)!;
                context.Database.AddPinProgress(new()
                {
                    PinId = relationToMigrate.PinId,
                    Progress = relationToMigrate.Progress,
                    PublisherId = relationToMigrate.PublisherId,
                    FirstPublished = relationToMigrate.FirstPublished,
                    LastUpdated = relationToMigrate.LastUpdated,
                    IsBeta = false, // doesn't matter here
                    Platform = TokenPlatform.Website,
                }, false);

                // Remove all others
                context.Database.RemovePinProgresses(group, false);
            }
        }

        context.Database.SaveChanges();
    }
}