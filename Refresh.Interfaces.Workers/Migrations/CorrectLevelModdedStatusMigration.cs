using Refresh.Core.Extensions;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers.Migrations;

public class CorrectLevelModdedStatusMigration : MigrationJob<GameLevel>
{
    protected override int BatchCount => 1000;

    protected override IQueryable<GameLevel> SortAndFilter(IQueryable<GameLevel> query)
    {
        return query
            .Where(l => l.GameVersion == TokenGame.LittleBigPlanetPSP) // for now this should only unflag PSP levels
            .OrderBy(l => l.LevelId);
    }

    protected override int Migrate(WorkContext context, GameLevel[] batch)
    {
        foreach (GameLevel level in batch)
        {
            context.Database.UpdateLevelModdedStatus(level, false);
        }

        context.Database.SaveChanges();
        return batch.Length;
    }
}