using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

public class NewestLevelsCategory : GameLevelCategory
{
    internal NewestLevelsCategory() : base("newest", "newest", false)
    {
        this.Name = "Новейшие уровни!";
        this.Description = "Уровни, которые были загружены совсем недавно!";
        this.IconHash = "g820623";
        this.FontAwesomeIcon = "calendar";
    }
    
    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => dataContext.Database.GetNewestLevels(count, skip, dataContext.User, levelFilterSettings);
}