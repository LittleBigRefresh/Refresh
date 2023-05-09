using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Matching;

[JsonObject(MemberSerialization.OptIn)]
public class GameRoom
{
    public GameRoom(GameUser host)
    {
        this.PlayerIds.Add(host.UserId);
    }
    
    [JsonProperty] public readonly List<ObjectId> PlayerIds = new(4);
    [JsonProperty] public ObjectId HostId => this.PlayerIds[0];

    public List<GameUser?> GetPlayers(GameDatabaseContext database) =>
        this.PlayerIds.Select(i => database.GetUserByObjectId(i))
            .ToList();

    public GameUser? GetHost(GameDatabaseContext database) => database.GetUserByObjectId(this.PlayerIds[0]);

    [JsonProperty("State"), JsonConverter(typeof(StringEnumConverter))]
    public RoomState RoomState;
    [JsonProperty("Mood"), JsonConverter(typeof(StringEnumConverter))]
    public RoomMood RoomMood;
    [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
    public RoomSlotType LevelType = RoomSlotType.Pod;
    
    [JsonProperty] public int LevelId;

}