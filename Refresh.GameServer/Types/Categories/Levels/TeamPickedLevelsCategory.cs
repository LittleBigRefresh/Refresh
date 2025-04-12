using Bunkum.Core;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Categories.Levels;

public class TeamPickedLevelsCategory : GameLevelCategory
{
    internal TeamPickedLevelsCategory() : base("teamPicked", "mmpicks", false)
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