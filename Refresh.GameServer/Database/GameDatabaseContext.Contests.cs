using JetBrains.Annotations;
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
    
    [Pure]
    public DatabaseList<GameLevel> GetLevelsFromContest(GameContest contest, int count, int skip, GameUser? user, LevelFilterSettings levelFilterSettings)
    {
        long start = contest.StartDate.ToUnixTimeMilliseconds();
        long end = contest.EndDate.ToUnixTimeMilliseconds();
        
        return new DatabaseList<GameLevel>(this.GetLevelsByGameVersion(levelFilterSettings.GameVersion)
            .FilterByLevelFilterSettings(user, levelFilterSettings)
            .Where(l => l.Title.Contains(contest.ContestTag))
            .Where(l => l.PublishDate > start && l.PublishDate < end), skip, count);
    }
}