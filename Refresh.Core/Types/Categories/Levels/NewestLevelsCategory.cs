using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

public class NewestLevelsCategory : GameCategory
{
    internal NewestLevelsCategory() : base("newest", "newest", false)
    {
        this.Name = "Новейшие уровни!";
        this.Description = "Уровни, которые были загружены совсем недавно!";
        this.IconHash = "g820623";
        this.FontAwesomeIcon = "calendar";
        this.PrimaryResultType = ResultType.Level;
    }
    
    public override DatabaseResultList? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => new(dataContext.Database.GetNewestLevels(count, skip, dataContext.User, levelFilterSettings));
}