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
                // Should never happen, but just incase
                if (!group.Any()) continue;

                // Find best one by the current user
                PinProgressRelation relationToMigrate = group.MaxBy(r => r.Progress)!;

                // Remove all already existing progresses to remove duplicates
                context.Database.RemoveAllPinProgressesByIdAndUser(pinId, relationToMigrate.PublisherId, false);

                // Now take the best progress we've just got and add it as a website pin, preserving other old metadata
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
            }
        }

        context.Database.SaveChanges();
    }
}