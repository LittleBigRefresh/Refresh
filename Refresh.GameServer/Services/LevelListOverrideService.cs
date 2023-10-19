using System.Diagnostics;
using Bunkum.Core.Services;
using MongoDB.Bson;
using NotEnoughLogs;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Services;

public class LevelListOverrideService : EndpointService
{
    public LevelListOverrideService(Logger logger) : base(logger)
    {}

    private readonly Dictionary<ObjectId, List<int>> _userIdsToLevelList = new(1);

    public bool UserHasOverrides(GameUser user) => this._userIdsToLevelList.ContainsKey(user.UserId);

    public void AddOverridesForUser(GameUser user, IEnumerable<GameLevel> levels)
    {
        Debug.Assert(!this.UserHasOverrides(user), "User already has overrides");
        
        List<int> ids = levels.Select(l => l.LevelId).ToList();
        this._userIdsToLevelList.Add(user.UserId, ids);
    }

    public IEnumerable<GameLevel> GetOverridesForUser(GameUser user, GameDatabaseContext database)
    {
        Debug.Assert(this.UserHasOverrides(user), "User does not have overrides, should be checked first");
        
        List<int> overrides = this._userIdsToLevelList[user.UserId];
        this._userIdsToLevelList.Remove(user.UserId);

        return overrides.Select(database.GetLevelById)!;
    }
}