using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Refresh.Database.Query;
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
        
        this.Write(() =>
        {
            contest.CreationDate = this._time.Now;
            this.GameContests.Add(contest);
        });
    }
    
    public void DeleteContest(GameContest contest)
    {
        this.Write(() =>
        {
            this.GameContests.Remove(contest);
        });
    }
    
    public GameContest? GetContestById(string? id)
    {
        if (id == null) return null;
        return this.GameContests.FirstOrDefault(c => c.ContestId == id);
    }
    
    public IEnumerable<GameContest> GetAllContests()
    {
        return this.GameContests
            .OrderBy(c => c.CreationDate);
    }
    
    public GameContest? GetNewestActiveContest()
    {
        DateTimeOffset now = this._time.Now;
        return this.GameContests
            .Where(c => c.StartDate <= now && c.EndDate > now) // Filter active contests
            .OrderByDescending(c => c.CreationDate)
            .FirstOrDefault();
    }
    
    public GameContest? GetLatestCompletedContest()
    {
        DateTimeOffset now = this._time.Now;
        return this.GameContests
            .Where(c => c.EndDate <= now)
            .OrderByDescending(c => c.EndDate)
            .FirstOrDefault();
    }
    
    public GameContest UpdateContest(ICreateContestInfo body, GameContest contest, GameUser? newOrganizer = null)
    {
        this.Write(() =>
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
            if (body.ContestTheme != null)
                contest.ContestTheme = body.ContestTheme;
            if (body.AllowedGames != null)
                contest.AllowedGames = body.AllowedGames;
            if (body.TemplateLevelId != null)
                contest.TemplateLevel = this.GetLevelById((int)body.TemplateLevelId);
        });
        
        return contest;
    }
    
    [Pure]
    public DatabaseList<GameLevel> GetLevelsFromContest(GameContest contest, int count, int skip, GameUser? user, LevelFilterSettings levelFilterSettings)
    {
        return new DatabaseList<GameLevel>(this.GetLevelsByGameVersion(levelFilterSettings.GameVersion)
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .Where(l => l.Title.Contains(contest.ContestTag))
            .Where(l => l.PublishDate >= contest.StartDate && l.PublishDate < contest.EndDate)
            .AsEnumerable() // This shouldn't be a noticeable performance hit, since levels that aren't for the contest have already been filtered out
            .Where(l => contest.AllowedGames.Contains(l.GameVersion)), skip, count);
    }
}