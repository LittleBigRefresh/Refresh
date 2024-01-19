using System.Diagnostics;
using Bunkum.Core.Services;
using MongoDB.Bson;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Services;

public class LevelListOverrideService : EndpointService
{
    public LevelListOverrideService(Logger logger) : base(logger)
    {}

    private readonly Dictionary<ObjectId, List<int>> _userIdsToLevelList = new(1);

    public bool UserHasOverrides(GameUser user)
    {
        bool result = this._userIdsToLevelList.ContainsKey(user.UserId);
        
        this.Logger.LogTrace(RefreshContext.LevelListOverride, "{0} has overrides: {1}", user.Username, result);
        return result;
    }

    public void AddOverridesForUser(GameUser user, GameLevel level)
    {
        this.AddOverridesForUser(user, new[] { level });
    }

    public void AddOverridesForUser(GameUser user, IEnumerable<GameLevel> levels)
    {
        Debug.Assert(!this.UserHasOverrides(user), "User already has overrides");
        
        List<int> ids = levels.Select(l => l.LevelId).ToList();
        this.Logger.LogDebug(RefreshContext.LevelListOverride, "Adding level override for {0}: [{1}]", user.Username, string.Join(", ", ids));
        this._userIdsToLevelList.Add(user.UserId, ids);
    }

    public IEnumerable<GameLevel> GetOverridesForUser(Token token, GameDatabaseContext database)
    {
        GameUser user = token.User;
        
        Debug.Assert(this.UserHasOverrides(user), "User does not have overrides, should be checked first");
        
        List<int> overrides = this._userIdsToLevelList[user.UserId];
        this.Logger.LogDebug(RefreshContext.LevelListOverride, "Getting level override for {0}: [{1}]", user.Username, string.Join(", ", overrides));
        this._userIdsToLevelList.Remove(user.UserId);

        List<GameLevel> levels = [];
        foreach (GameLevel level in overrides.Select(levelId => database.GetLevelById(levelId)!))
        {
            //If the game cannot play this level, skip it
            if (!token.TokenGame.CanPlay(level)) 
                continue;

            levels.Add(level);
        }
        
        return levels;
    }
}