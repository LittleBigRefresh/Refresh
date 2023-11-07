using Bunkum.Core;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

public class CoolLevelsCategory : LevelCategory
{
    public CoolLevelsCategory() : base("coolLevels", new []{"lbpcool", "lbp2cool"}, false)
    {
        this.Name = "Cool Levels";
        this.Description = "Levels trending with players like you!";
        this.FontAwesomeIcon = "fireAlt";
        this.IconHash = "g820625";
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count, MatchService matchService,
        GameDatabaseContext database, GameUser? user, LevelFilterSettings levelFilterSettings)
    {
        return database.GetCoolLevels(count, skip, user, levelFilterSettings);
    }
}