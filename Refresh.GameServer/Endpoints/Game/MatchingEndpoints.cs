using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class MatchingEndpoints : EndpointGroup
{
    // [FindBestRoom,["Players":["VitaGamer128"],"Reservations":["0"],"NAT":[2],"Slots":[[5,0]],"Location":[0x17257bc9,0x17257bf2],"Language":1,"BuildVersion":289,"Search":"","RoomState":3]]
    [GameEndpoint("match", HttpMethods.Post, ContentType.Json)]
    public Response Match(RequestContext context, GameDatabaseContext database, GameUser user, Token token, MatchService service, string body)
    {
        (string method, string jsonBody) = MatchService.ExtractMethodAndBodyFromJson(body);
        context.Logger.LogDebug(BunkumCategory.Matching, $"Received {method} match request, data: {jsonBody}");
        
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
        
        return service.ExecuteMethod(method, roomData, database, user, token);
    }
}