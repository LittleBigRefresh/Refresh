using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

public class HighestRatedLevelsCategory : GameCategory
{
    internal HighestRatedLevelsCategory() : base("mostLiked", ["thumbs", "highestRated"], false)
    {
        this.Name = "Highest Rated";
        this.Description = "Levels with the most Yays!";
        this.FontAwesomeIcon = "thumbs-up";
        this.IconHash = "g820603";
        this.PrimaryResultType = ResultType.Level;
    }

    public override DatabaseResultList? Fetch(RequestContext context, int skip, int count,
        DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => new(dataContext.Database.GetHighestRatedLevels(count, skip, dataContext.User, levelFilterSettings));
}