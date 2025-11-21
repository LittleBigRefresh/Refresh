using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

public class MostReplayedLevelsCategory : GameCategory
{
    internal MostReplayedLevelsCategory() : base("mostReplayed", "mostPlays", false)
    {
        this.Name = "Replayable Levels";
        this.Description = "Levels people love to play over and over!";
        this.FontAwesomeIcon = "forward";
        this.IconHash = "g820608";
        this.PrimaryResultType = ResultType.Level;
    }

    public override DatabaseResultList? Fetch(RequestContext context, int skip, int count,
        DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => new(dataContext.Database.GetMostReplayedLevels(count, skip, dataContext.User, levelFilterSettings));
}