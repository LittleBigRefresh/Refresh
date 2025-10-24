using Refresh.Database.Extensions;
using Refresh.Database.Models.Users;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers.Migrations;

public class BackfillModdedPlanetFlagsMigration : MigrationJob<GameUser>
{
    protected override IQueryable<GameUser> SortAndFilter(IQueryable<GameUser> query)
    {
        return query
            .OrderBy(u => u.UserId)
            .Where(u => u.Lbp2PlanetsHash != "0" 
            || u.Lbp3PlanetsHash != "0" 
            || u.VitaPlanetsHash != "0" 
            || u.BetaPlanetsHash != "0");
    }

    protected override void Migrate(WorkContext context, GameUser[] batch)
    {
        foreach (GameUser user in batch)
        {
            context.Database.UpdatePlanetModdedStatus(user);
        }

        context.Database.SaveChanges();
    }
}