using Bunkum.Core.Services;
using MongoDB.Bson;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.Database;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
using GameDatabaseContext = Refresh.Database.GameDatabaseContext;

namespace Refresh.GameServer.Services;

public class PlayNowService : EndpointService
{
    private PresenceService _presence;
    
    public PlayNowService(Logger logger, PresenceService presence) : base(logger)
    {
        this._presence = presence;
    }

    private readonly Dictionary<ObjectId, List<int>> _userIdsToLevelList = new(1);
    private readonly Dictionary<ObjectId, (bool accessed, string hash)> _userIdsToLevelHash = new(1);

    private bool UserHasLevelIdOverrides(GameUser user)
    {
        bool result = this._userIdsToLevelList.ContainsKey(user.UserId);
        
        this.Logger.LogTrace(RefreshContext.LevelListOverride, "{0} has id overrides: {1}", user.Username, result);
        return result;
    }
    
    private bool UserHasLevelHashOverride(GameUser user)
    {
        bool result;
        
        if (this._userIdsToLevelHash.TryGetValue(user.UserId, out (bool accessed, string hash) value))
            result = !value.accessed;
        else
            result = false;
        
        this.Logger.LogTrace(RefreshContext.LevelListOverride, "{0} has hash override: {1}", user.Username, result);
        return result; 
    }
    
    public bool UserHasOverrides(GameUser user) 
        => this.UserHasLevelHashOverride(user) || this.UserHasLevelIdOverrides(user);
    
    public bool PlayNowHash(GameUser user, string hash)
    {
        this.Logger.LogDebug(RefreshContext.LevelListOverride, "Adding level hash override for {0}: [{1}]", user.Username, hash);

        bool presenceUsed = this._presence.PlayLevel(user, GameLevelResponse.LevelIdFromHash(hash));
        
        // Set the hash override, but mark it as already accessed if presence was used
        this._userIdsToLevelHash[user.UserId] = (presenceUsed, hash);

        return presenceUsed;
    }
    
    public bool GetLastHashOverrideForUser(Token token, out string hash)
    {
        GameUser user = token.User;
        
        if (!this._userIdsToLevelHash.TryGetValue(user.UserId, out (bool accessed, string hash) value))
        {
            hash = null!;
            return false;
        }
        
        hash = value.hash;
        
        return true;
    }
    
    public bool GetHashOverrideForUser(Token token, out string hash)
    {
        GameUser user = token.User;
        
        if (!this.UserHasLevelHashOverride(user))
        {
            hash = null!;
            return false;
        }
        
        (bool accessed, string hash) overrides = this._userIdsToLevelHash[user.UserId];
        
        if (overrides.accessed)
        {
            hash = null!;
            return false;
        }
        
        this.Logger.LogDebug(RefreshContext.LevelListOverride, "Getting level hash override for {0}: [{1}]", user.Username, overrides.hash);
        
        hash = overrides.hash;
        
        this._userIdsToLevelHash[user.UserId] = (true, overrides.hash);
        
        return true;
    }

    public bool PlayNowLevel(GameUser user, GameLevel level)
    {
        if (this._presence.PlayLevel(user, level.LevelId)) 
            return true;
        
        this.AddIdOverridesForUser(user, [level]);
        return false;
    }
    
    public void AddIdOverridesForUser(GameUser user, IEnumerable<GameLevel> levels)
    {
        List<int> ids = levels.Select(l => l.LevelId).ToList();
        this.Logger.LogDebug(RefreshContext.LevelListOverride, "Adding level id overrides for {0}: [{1}]", user.Username, string.Join(", ", ids));
        this._userIdsToLevelList[user.UserId] = ids;
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