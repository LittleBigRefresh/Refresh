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
    
    public GameContest? GetOldestActiveContest()
    {
        DateTimeOffset now = this._time.Now;
        return this._realm.All<GameContest>()
            .Where(c => c.StartDate <= now && c.EndDate > now) // Filter active contests
            .OrderBy(c => c.CreationDate)
            .FirstOrDefault();
    }
    
    public GameContest UpdateContest(ApiContestRequest body, GameContest contest, GameUser? newOrganizer = null)
    {
        this._realm.Write(() =>
        {
            PropertyInfo[] bodyProps = typeof(ApiContestRequest).GetProperties();
            foreach (PropertyInfo prop in bodyProps)
            {
                if (!prop.CanWrite || !prop.CanRead) continue;
                if(prop.Name == nameof(ApiContestRequest.OrganizerId)) continue;
                
                object? propValue = prop.GetValue(body);
                if(propValue == null) continue;
                
                PropertyInfo? gameContestProp = typeof(GameContest).GetProperty(prop.Name);
                Debug.Assert(gameContestProp != null, $"Invalid property {prop.Name} on {nameof(ApiContestRequest)}");
                
                gameContestProp.SetValue(contest, prop.GetValue(body));
            }
            
            if (newOrganizer != null)
                contest.Organizer = newOrganizer;
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