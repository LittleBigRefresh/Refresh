using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

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