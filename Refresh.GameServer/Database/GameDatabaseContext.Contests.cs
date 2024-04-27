using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Contests;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Contests
{
    public void CreateContest(GameContest contest)
    {
        if (this.GetContestById(contest.ContestId) != null) throw new InvalidOperationException("Contest already exists.");
        
        this._realm.Write(() =>
        {
            contest.CreationDate = this._time.Now;
            this._realm.Add(contest);
        });
    }
    
    public GameContest? GetContestById(string? id)
    {
        if (id == null) return null;
        return this._realm.All<GameContest>().FirstOrDefault(c => c.ContestId == id);
    }
    
    public IEnumerable<GameContest> GetAllContests()
    {
        return this._realm.All<GameContest>()
            .OrderBy(c => c.CreationDate);
    }
    
    public GameContest? GetNewestActiveContest()
    {
        DateTimeOffset now = this._time.Now;
        return this._realm.All<GameContest>()
            .Where(c => c.StartDate <= now && c.EndDate > now) // Filter active contests
            .OrderByDescending(c => c.CreationDate)
            .FirstOrDefault();
    }
    
    public GameContest UpdateContest(ApiContestRequest body, GameContest contest, GameUser? newOrganizer = null)
    {
        this._realm.Write(() =>
        {
            if (newOrganizer != null)
                contest.Organizer = newOrganizer;
            
            if(body.StartDate != null)
                contest.StartDate = body.StartDate.Value;
            if(body.EndDate != null)
                contest.EndDate = body.EndDate.Value;
            if(body.ContestTag != null)
                contest.ContestTag = body.ContestTag;
            if(body.BannerUrl != null)
                contest.BannerUrl = body.BannerUrl;
            if(body.ContestTitle != null)
                contest.ContestTitle = body.ContestTitle;
            if(body.ContestSummary != null)
                contest.ContestSummary = body.ContestSummary;
            if(body.ContestDetails != null)
                contest.ContestDetails = body.ContestDetails;
        });
        
        return contest;
    }
    
    [Pure]
    public DatabaseList<GameLevel> GetLevelsFromContest(GameContest contest, int count, int skip, GameUser? user, LevelFilterSettings levelFilterSettings)
    {
        long start = contest.StartDate.ToUnixTimeMilliseconds();
        long end = contest.EndDate.ToUnixTimeMilliseconds();
        
        return new DatabaseList<GameLevel>(this.GetLevelsByGameVersion(levelFilterSettings.GameVersion)
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .Where(l => l.Title.Contains(contest.ContestTag))
            .Where(l => l.PublishDate >= start && l.PublishDate < end), skip, count);
    }
}