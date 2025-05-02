using Bunkum.Core;
using Refresh.Database.Query;
using Refresh.Database;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Categories.Levels;

public class HighestRatedLevelsCategory : GameLevelCategory
{
    internal HighestRatedLevelsCategory() : base("mostLiked", ["thumbs", "highestRated"], false)
    {
        this.Name = "Highest Rated";
        this.Description = "Levels with the most Yays!";
        this.FontAwesomeIcon = "thumbs-up";
        this.IconHash = "g820603";
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count,
        DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => dataContext.Database.GetHighestRatedLevels(count, skip, dataContext.User, levelFilterSettings);
}