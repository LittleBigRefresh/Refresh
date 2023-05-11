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
        this.PlayerIds.Add(new GameRoomPlayer(host.Username, host.UserId));
    }

    [JsonProperty] public readonly ObjectId RoomId = ObjectId.GenerateNewId();
    
    [JsonProperty] public readonly List<GameRoomPlayer> PlayerIds = new(4);
    [JsonProperty] public GameRoomPlayer HostId => this.PlayerIds[0];

    public List<GameUser?> GetPlayers(GameDatabaseContext database) =>
        this.PlayerIds
            .Where(i => i.Id != null)
            .Select(i => database.GetUserByObjectId(i.Id))
            .ToList();

    public GameUser? GetHost(GameDatabaseContext database)
    {
        if (this.PlayerIds[0].Id == null) return null;
        return database.GetUserByObjectId(this.PlayerIds[0].Id);
    }

    [JsonProperty("State"), JsonConverter(typeof(StringEnumConverter))]
    public RoomState RoomState;
    [JsonProperty("Mood"), JsonConverter(typeof(StringEnumConverter))]
    public RoomMood RoomMood;
    [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
    public RoomSlotType LevelType = RoomSlotType.Pod;
    
    [JsonProperty] public int LevelId;

}