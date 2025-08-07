using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

public class TeamPickedLevelsCategory : GameLevelCategory
{
    internal TeamPickedLevelsCategory() : base("teamPicks", "mmpicks", false)
    {
        this.Name = "Team Picks";
        this.Description = "High quality levels, hand-picked by us.";
        this.FontAwesomeIcon = "certificate";
        this.IconHash = "g820626";
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count,
        DataContext dataContext,
        ResultFilterSettings levelFilterSettings, GameUser? _) 
        => dataContext.Database.GetTeamPickedLevels(count, skip, dataContext.User, levelFilterSettings);
}