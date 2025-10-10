using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers.Migrations;

public class ClampPlayerLimitsMigration : MigrationJob<GameLevel>
{
    protected override int BatchCount => 1000;

    protected override IQueryable<GameLevel> SortAndFilter(IQueryable<GameLevel> query)
    {
        return query.OrderBy(l => l.LevelId);
    }

    protected override void Migrate(WorkContext context, GameLevel[] batch)
    {
        foreach (GameLevel level in batch)
        {
            if (level.MinPlayers == 0 || level.GameVersion == TokenGame.LittleBigPlanet1)
                level.MinPlayers = 1;

            if (level.MaxPlayers != 4 && level.GameVersion == TokenGame.LittleBigPlanet1)
                level.MaxPlayers = 4;
            
            level.MinPlayers = Math.Clamp(level.MinPlayers, 1, 4);
            level.MaxPlayers = Math.Clamp(level.MaxPlayers, 1, 4);
        }
    }
}