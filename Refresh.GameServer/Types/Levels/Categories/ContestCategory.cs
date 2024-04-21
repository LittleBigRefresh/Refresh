using Bunkum.Core;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Contests;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

public class ContestCategory : LevelCategory
{
    public ContestCategory() : base("contest", Array.Empty<string>(), false)
    {
        this.Name = "Contests";
        this.Description = "Levels from a contest.";
        this.FontAwesomeIcon = "certificate";
        this.IconHash = "g820608";
        this.Hidden = true;
    }
    
    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count, MatchService matchService,
        GameDatabaseContext database, GameUser? user, LevelFilterSettings levelFilterSettings)
    {
        string? contestId = context.QueryString["contest"];
        GameContest? contest = database.GetContestById(contestId);
        if (contest == null) return null;
        
        return database.GetLevelsFromContest(contest, count, skip, user, levelFilterSettings);
    }
}