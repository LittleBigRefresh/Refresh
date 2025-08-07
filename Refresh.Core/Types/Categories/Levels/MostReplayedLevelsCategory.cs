using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

public class MostReplayedLevelsCategory : GameLevelCategory
{
    internal MostReplayedLevelsCategory() : base("mostReplayed", "mostPlays", false)
    {
        this.Name = "Replayable Levels";
        this.Description = "Levels people love to play over and over!";
        this.FontAwesomeIcon = "forward";
        this.IconHash = "g820608";
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count,
        DataContext dataContext,
        ResultFilterSettings levelFilterSettings, GameUser? _) 
        => dataContext.Database.GetMostReplayedLevels(count, skip, dataContext.User, levelFilterSettings);
}