using System.Net;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using MongoDB.Bson;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Legacy;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.LegacyApi;

public class LegacyApiEndpoints : EndpointGroup
{
    [LegacyApiEndpoint("username/{username}")]
    [Authentication(false)]
    public LegacyGameUser? GetLegacyUserByUsername(RequestContext context, GameDatabaseContext database, string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return null;
        
        return LegacyGameUser.FromGameUser(user);
    }
    
    [LegacyApiEndpoint("user/{idStr}")]
    [Authentication(false)]
    public LegacyGameUser? GetLegacyUserByLegacyId(RequestContext context, GameDatabaseContext database, string idStr)
    {
        int.TryParse(idStr, out int id);
        if (id == default) return null;
        
        GameUser? user = database.GetUserByLegacyId(id);
        if (user == null) return null;
        
        return LegacyGameUser.FromGameUser(user);
    }

    [LegacyApiEndpoint("user/{idStr}/status")]
    [Authentication(false)]
    public LegacyStatus? GetLegacyUserStatus(RequestContext context, MatchService match, GameDatabaseContext database, string idStr)
    {
        _ = int.TryParse(idStr, out int id);
        if (id == default) return null;

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
        };
    }
}