using Bunkum.Core;
using Refresh.Database.Query;
using Refresh.Database;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Categories.Levels;

public class MostHeartedLevelsCategory : GameLevelCategory
{
    internal MostHeartedLevelsCategory() : base("mostHearted", "mostHearted", false)
    {
        this.Name = "Community's Favorites";
        this.Description = "The all-time most hearted levels!";
        this.FontAwesomeIcon = "heart";
        this.IconHash = "g820607";
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count,
        DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => dataContext.Database.GetMostHeartedLevels(count, skip, dataContext.User, levelFilterSettings);
}