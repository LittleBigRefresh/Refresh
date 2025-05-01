using Bunkum.Core;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Categories.Levels;

public class MostUniquelyPlayedLevelsCategory : GameLevelCategory
{
    internal MostUniquelyPlayedLevelsCategory() : base("mostPlayed", "mostUniquePlays", false)
    {
        this.Name = "Starter Pack";
        this.Description = "Levels that many people have played.";
        this.FontAwesomeIcon = "play";
        this.IconHash = "g820608";
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count,
        DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => dataContext.Database.GetMostUniquelyPlayedLevels(count, skip, dataContext.User, levelFilterSettings);
}