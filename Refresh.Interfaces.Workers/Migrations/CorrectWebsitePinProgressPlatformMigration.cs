using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Pins;
using Refresh.Database.Models.Relations;
using Refresh.Database.Models.Users;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers.Migrations;

// Easier to do this by user than by pin relation, to avoid various pagination issues
public class CorrectWebsitePinProgressPlatformMigration : MigrationJob<GameUser>
{
    private readonly List<long> WebsitePinIds =
    [
        (long)ServerPins.HeartPlayerOnWebsite,
        (long)ServerPins.QueueLevelOnWebsite,
        (long)ServerPins.SignIntoWebsite,
    ];

    protected override IQueryable<GameUser> SortAndFilter(IQueryable<GameUser> query)
    {
        return query.OrderBy(u => u.UserId);
    }

    protected override int Migrate(WorkContext context, GameUser[] batch)
    {
        foreach (GameUser user in batch)
        {
            foreach (long pinId in this.WebsitePinIds)
            {
                List<PinProgressRelation> pinsByUserAndId = context.Database.GetAllPinProgressesByUserAndId(user, pinId).ToList();
                if (pinsByUserAndId.Count <= 0) continue; // no need to deduplicate if there is nothing (do still migrate if there's only 1 pin, to overwrite its platform)

                PinProgressRelation relationToMigrate = pinsByUserAndId.MaxBy(r => r.Progress)!;

                foreach (PinProgressRelation relation in pinsByUserAndId)
                {
                    context.Database.RemovePinProgress(relation, false);
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
            }
        }

        context.Database.SaveChanges();
        return batch.Length;
    }
}