using Bunkum.Core;
using Refresh.Database.Query;
using Refresh.Database;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Categories.Levels;

public class NewestLevelsCategory : GameLevelCategory
{
    internal NewestLevelsCategory() : base("newest", "newest", false)
    {
        this.Name = "Newest Levels";
        this.Description = "Levels that were most recently uploaded!";
        this.IconHash = "g820623";
        this.FontAwesomeIcon = "calendar";
    }
    
    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => dataContext.Database.GetNewestLevels(count, skip, dataContext.User, levelFilterSettings);
}