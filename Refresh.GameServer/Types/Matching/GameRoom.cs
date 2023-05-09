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
        this._playerIds.Add(host.UserId);
    }
    
    [JsonProperty("PlayerIds")]
    private readonly List<ObjectId> _playerIds = new(4);
    // ReSharper disable once InconsistentNaming
    [JsonProperty("HostId")]
    private ObjectId _hostId => this._playerIds[0];

    public List<GameUser?> GetPlayers(GameDatabaseContext database) =>
        this._playerIds.Select(i => database.GetUserByObjectId(i))
            .ToList();

    public GameUser? GetHost(GameDatabaseContext database) => database.GetUserByObjectId(this._playerIds[0]);

    [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
    public RoomState RoomState;
    [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
    public RoomSlotType LevelType;
    
    [JsonProperty] public int LevelId;
}