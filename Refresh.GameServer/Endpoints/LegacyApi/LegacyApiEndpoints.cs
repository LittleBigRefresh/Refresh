using Bunkum.Core;
using Bunkum.Core.Endpoints;
using MongoDB.Bson;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Legacy;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.RichPresence;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.LegacyApi;

public class LegacyApiEndpoints : EndpointGroup
{
    [LegacyApiEndpoint("rpc"), Authentication(false)]
    public RichPresenceConfiguration GetRichPresenceConfiguration(RequestContext context, GameServerConfig gameConfig, RichPresenceConfig richConfig)
        => RichPresenceConfiguration.Create(gameConfig, richConfig, true);

    [LegacyApiEndpoint("username/{username}")]
    [Authentication(false)]
    public LegacyGameUser? GetLegacyUserByUsername(RequestContext context, IGameDatabaseContext database, string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return null;
        
        return LegacyGameUser.FromOld(user);
    }
    
    [LegacyApiEndpoint("user/{id}")]
    [Authentication(false)]
    public LegacyGameUser? GetLegacyUserByLegacyId(RequestContext context, IGameDatabaseContext database, int id)
    {
        GameUser? user = database.GetUserByLegacyId(id);
        if (user == null) return null;
        
        return LegacyGameUser.FromOld(user);
    }

    [LegacyApiEndpoint("slot/{id}")]
    [Authentication(false)]
    public LegacyGameLevel? GetLegacyLevel(RequestContext context, IGameDatabaseContext database, int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return null;
        
        return LegacyGameLevel.FromOld(level);
    }

    [LegacyApiEndpoint("user/{id}/status")]
    [Authentication(false)]
    public LegacyStatus? GetLegacyUserStatus(RequestContext context, MatchService match, IGameDatabaseContext database, int id)
    {
        GameUser? user = database.GetUserByLegacyId(id);
        if (user == null) return null;

        GameRoom? room = match.GetRoomByPlayer(user);
        if (room == null) return null;

        List<int> playerIds = new();
        foreach ((string? _, ObjectId? playerId) in room.PlayerIds)
        {
            if(playerId != null) playerIds.Add(playerId.Value.Timestamp);
        }

        return new LegacyStatus
        {
            CurrentRoom = new LegacyRoom
            {
                RoomId = room.RoomId.Timestamp,
                Slot = new LegacyRoomSlot
                {
                    SlotId = room.LevelId,
                    SlotType = (int)room.LevelType,
                },
                PlayerIds = playerIds.ToArray(),
            },
            CurrentPlatform = 1,
            CurrentVersion = 1,
            LastLogin = user.LastLoginDate.ToUnixTimeMilliseconds(),
            LastLogout = user.LastLoginDate.ToUnixTimeMilliseconds() + 1,
        };
    }
}