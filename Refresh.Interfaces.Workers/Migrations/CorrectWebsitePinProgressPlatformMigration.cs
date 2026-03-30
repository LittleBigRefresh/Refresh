using MongoDB.Bson;
using Refresh.Common.Time;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Pins;
using Refresh.Database.Models.Relations;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers.Migrations;

public class CorrectWebsitePinProgressPlatformMigration : MigrationJob<PinProgressRelation>
{
    private readonly List<long> WebsitePinIds =
    [
        (long)ServerPins.HeartPlayerOnWebsite,
        (long)ServerPins.QueueLevelOnWebsite,
        (long)ServerPins.SignIntoWebsite,
    ];

    protected override IQueryable<PinProgressRelation> SortAndFilter(IQueryable<PinProgressRelation> query)
    {
        return query
            .Where(p => this.WebsitePinIds.Contains(p.PinId))
            .OrderBy(p => p.PinId);
    }

    protected override int Migrate(WorkContext context, PinProgressRelation[] batch)
    {
        int pinsLeft = batch.Length;

        foreach (long pinId in this.WebsitePinIds)
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
                List<PinProgressRelation> relationsToRemove = group.ToList();

                foreach (PinProgressRelation relation in group)
                {
                    context.Database.RemovePinProgress(relation, false);
                    pinsLeft--;
                }

                // Now take the best progress we've just got and add it as a website pin, preserving other old metadata
                PinProgressRelation newRelation = new()
                {
                    PinId = relationToMigrate.PinId,
                    Progress = relationToMigrate.Progress,
                    PublisherId = relationToMigrate.PublisherId,
                    FirstPublished = relationToMigrate.FirstPublished,
                    LastUpdated = relationToMigrate.LastUpdated,
                    IsBeta = false, // doesn't matter here
                    Platform = TokenPlatform.Website,
                };

                context.Database.AddPinProgress(newRelation, false);
                pinsLeft++;
            }
        }

        context.Database.SaveChanges();
        return pinsLeft;
    }
}