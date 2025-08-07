using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Contests;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

public class ContestCategory : GameLevelCategory
{
    public ContestCategory() : base("contest", [], false)
    {
        this.Name = "Contests";
        this.Description = "Levels from a contest.";
        this.FontAwesomeIcon = "certificate";
        this.IconHash = "g820608";
    }
    
    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        ResultFilterSettings levelFilterSettings, GameUser? _)
    {
        // try to find a contest by the query parameter
        string? contestId = context.QueryString["contest"];
        GameContest? contest = dataContext.Database.GetContestById(contestId);
        
        // if we can't find one by a query param, try getting an active contest instead
        contest ??= dataContext.Database.GetNewestActiveContest();

        // if there's no active contest, then try to use the last one that ended
        contest ??= dataContext.Database.GetLatestCompletedContest();
        
        // if not, then fail
        if (contest == null)
            return null;
        
        return dataContext.Database.GetLevelsFromContest(contest, count, skip, dataContext.User, levelFilterSettings);
    }
}