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
    private readonly Dictionary<ObjectId, List<string>> _userIdsToLevelHashList = new(1);

    private bool UserHasLevelIdOverrides(GameUser user)
    {
        bool result = this._userIdsToLevelList.ContainsKey(user.UserId);
        
        this.Logger.LogTrace(RefreshContext.LevelListOverride, "{0} has id overrides: {1}", user.Username, result);
        return result;
    }
    
    private bool UserHasLevelHashOverrides(GameUser user)
    {
        bool result = this._userIdsToLevelHashList.ContainsKey(user.UserId);
        
        this.Logger.LogTrace(RefreshContext.LevelListOverride, "{0} has hash overrides: {1}", user.Username, result);
        return result; 
    }
    
    public bool UserHasOverrides(GameUser user) 
        => this.UserHasLevelHashOverrides(user) || this.UserHasLevelIdOverrides(user);
    
    public void AddHashOverridesForUser(GameUser user, string hash) 
        => this.AddHashOverridesForUser(user, new[] { hash });
    
    public void AddHashOverridesForUser(GameUser user, IEnumerable<string> hashes)
    {
        Debug.Assert(!this.UserHasLevelHashOverrides(user), "User already has overrides");
        
        List<string> hashList = hashes.ToList();
        
        this.Logger.LogDebug(RefreshContext.LevelListOverride, "Adding level hash overrides for {0}: [{1}]", user.Username, string.Join(", ", hashList));
        this._userIdsToLevelHashList.Add(user.UserId, hashList);
    }
    
    public bool GetHashOverridesForUser(Token token, out IEnumerable<string> hashes)
    {
        GameUser user = token.User;
        
        if (!this.UserHasLevelHashOverrides(user))
        {
            hashes = null!;
            return false;
        }
        
        List<string> overrides = this._userIdsToLevelHashList[user.UserId].ToList();
        this.Logger.LogDebug(RefreshContext.LevelListOverride, "Getting level hash overrides for {0}: [{1}]", user.Username, string.Join(", ", overrides));
        this._userIdsToLevelHashList.Remove(user.UserId);
        
        hashes = overrides;
        
        return true;
    } 
    
    public void AddIdOverridesForUser(GameUser user, GameLevel level) 
        => this.AddIdOverridesForUser(user, new[] { level });
    
    public void AddIdOverridesForUser(GameUser user, IEnumerable<GameLevel> levels)
    {
        Debug.Assert(!this.UserHasLevelIdOverrides(user), "User already has overrides");
        
        List<int> ids = levels.Select(l => l.LevelId).ToList();
        this.Logger.LogDebug(RefreshContext.LevelListOverride, "Adding level id overrides for {0}: [{1}]", user.Username, string.Join(", ", ids));
        this._userIdsToLevelList.Add(user.UserId, ids);
    }

    public bool GetIdOverridesForUser(Token token, GameDatabaseContext database, out IEnumerable<GameLevel> outLevels)
    {
        GameUser user = token.User;
        
        if (!this.UserHasLevelIdOverrides(user))
        {
            outLevels = null!;
            return false;
        } 
        
        List<int> overrides = this._userIdsToLevelList[user.UserId];
        this.Logger.LogDebug(RefreshContext.LevelListOverride, "Getting level id overrides for {0}: [{1}]", user.Username, string.Join(", ", overrides));
        this._userIdsToLevelList.Remove(user.UserId);

        List<GameLevel> levels = [];
        foreach (GameLevel level in overrides.Select(levelId => database.GetLevelById(levelId)!))
        {
            //If the game cannot play this level, skip it
            if (!token.TokenGame.CanPlay(level)) 
                continue;

            levels.Add(level);
        }
        
        outLevels = levels;
        
        return true;
    }
}