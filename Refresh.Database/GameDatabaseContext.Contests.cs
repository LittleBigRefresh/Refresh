using JetBrains.Annotations;
using Refresh.Database.Query;
using Refresh.Database.Models.Contests;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;

namespace Refresh.Database;

public partial class GameDatabaseContext // Contests
{
    public GameContest CreateContest(string contestId, ICreateContestInfo createInfo, GameUser organizer, GameLevel? templateLevel = null)
    {
        if (this.GetContestById(contestId) != null) throw new InvalidOperationException("Contest already exists.");
        
        GameContest contest = new()
        {
            ContestId = contestId,
            Organizer = organizer,
            CreationDate = this._time.Now,
            StartDate = createInfo.StartDate!.Value,
            EndDate = createInfo.EndDate!.Value,
            ContestTitle = createInfo.ContestTitle!,
            BannerUrl = createInfo.BannerUrl ?? "",
            ContestTag = createInfo.ContestTag ?? $"#{contestId}",
            ContestSummary = createInfo.ContestSummary ?? "",
            ContestDetails = createInfo.ContestDetails ?? "",
            ContestTheme = createInfo.ContestTheme ?? "",
            AllowedGames = createInfo.AllowedGames ?? [],
            TemplateLevel = templateLevel
        };

        this.GameContests.Add(contest);
        this.SaveChanges();
        return contest;
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
    
    public GameContest UpdateContest(ICreateContestInfo body, GameContest contest, GameUser? newOrganizer = null, GameLevel? newTemplate = null)
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

            if (newTemplate != null)
                contest.TemplateLevel = newTemplate;
        });
        
        return contest;
    }
    
    [Pure]
    public DatabaseList<GameLevel> GetLevelsFromContest(GameContest contest, int count, int skip, GameUser? user, LevelFilterSettings levelFilterSettings)
    {
        IQueryable<GameLevel> levels = this.GetLevelsByGameVersion(levelFilterSettings.GameVersion)
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .Where(l => l.Title.Contains(contest.ContestTag))
            .Where(l => l.PublishDate >= contest.StartDate && l.PublishDate < contest.EndDate);
        
        // Allow levels from all games if there are no allowed games specified
        if (contest.AllowedGames.Count > 0)
        {
            levels = levels.Where(l => contest.AllowedGames.Contains(l.GameVersion));
        }

        return new(levels.ToArray(), skip, count);
    }
}