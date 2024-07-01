using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Endpoints.Debugging;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class MatchingEndpoints : EndpointGroup
{
    // [FindBestRoom,["Players":["VitaGamer128"],"Reservations":["0"],"NAT":[2],"Slots":[[5,0]],"Location":[0x17257bc9,0x17257bf2],"Language":1,"BuildVersion":289,"Search":"","RoomState":3]]
    [GameEndpoint("match", HttpMethods.Post, ContentType.Json)]
    [DebugRequestBody, DebugResponseBody]
    public Response Match(
        RequestContext context, 
        GameDatabaseContext database, 
        GameUser user, 
        Token token, 
        MatchService service, 
        string body, 
        GameServerConfig gameServerConfig)
    {
        (string method, string jsonBody) = MatchService.ExtractMethodAndBodyFromJson(body);
        context.Logger.LogInfo(BunkumCategory.Matching, $"Received {method} match request, data: {jsonBody}");
        
        JsonSerializer serializer = new();
        using StringReader reader = new(jsonBody);
        using JsonTextReader jsonReader = new(reader);

        SerializedRoomData? roomData = serializer.Deserialize<SerializedRoomData>(jsonReader);
        
        // ReSharper disable once InvertIf (happy path goes last)
        if (roomData == null)
        {
            context.Logger.LogWarning(BunkumCategory.Matching, "Match data was bad and unserializable, rejecting."); // Already logged data
            return BadRequest;
        }
        
        return service.ExecuteMethod(method, roomData, database, user, token, gameServerConfig);
    }
    
    // Sent by LBP1 to notify the server it has entered a level.
    // While it doesn't send us any detailed room information, we can at least create a pseudo-room on the server with just that player.
    // Due to it only sending this when the level is *entered*, it means its much more likely for the room to be auto cleared due to inactivity,
    // since the "bump" is much less often. This will at the very least make API tools be able to see LBP1 player activity and player counts
    [GameEndpoint("enterLevel/{slotType}/{id}", HttpMethods.Post)]
    public Response EnterLevel(RequestContext context, Token token, MatchService matchService, string slotType, int id)
    {
        GameRoom room = matchService.GetOrCreateRoomByPlayer(token.User, token.TokenPlatform, token.TokenGame, NatType.Strict, null, false);
        
        // User slot ID of 0 means pod/moon level
        if (id == 0 && slotType == "user")
        {
            // We cant determine whether the user is in a pod or moon level, so we just assume it's the pod
            room.LevelType = RoomSlotType.Pod;
            room.LevelId = id;
        }
        else
        {
            room.LevelType = slotType == "user" ? RoomSlotType.Online : RoomSlotType.Story;
            room.LevelId = id;
        }
        
        room.LastContact = DateTimeOffset.Now;
        
        matchService.RoomAccessor.UpdateRoom(room);
        
        return OK;
    }
}