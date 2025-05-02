using Bunkum.Core;
using Refresh.Database.Query;
using Refresh.Database;
using Refresh.GameServer.Types.Data;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;

namespace Refresh.GameServer.Types.Categories.Levels;

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
        LevelFilterSettings levelFilterSettings, GameUser? _) 
        => dataContext.Database.GetTeamPickedLevels(count, skip, dataContext.User, levelFilterSettings);
}