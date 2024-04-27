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
    }
    
    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count, MatchService matchService,
        GameDatabaseContext database, GameUser? accessor, LevelFilterSettings levelFilterSettings, GameUser? _)
    {
        // try to find a contest by the query parameter
        string? contestId = context.QueryString["contest"];
        GameContest? contest = database.GetContestById(contestId);
        
        // if we cant find one by a query param try getting an active contest instead
        contest ??= database.GetNewestActiveContest();
        
        // if not, then fail
        if (contest == null)
            return null;
        
        return database.GetLevelsFromContest(contest, count, skip, accessor, levelFilterSettings);
    }
}