using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Matching;

public class GameRoom
{
    public GameRoom(GameUser host, TokenPlatform platform, TokenGame game)
    {
        this.PlayerIds.Add(new GameRoomPlayer(host.Username, host.UserId));
        this.Platform = platform;
        this.Game = game;
    }

    public readonly ObjectId RoomId = ObjectId.GenerateNewId();
    
    public readonly List<GameRoomPlayer> PlayerIds = new(4);
    public GameRoomPlayer HostId => this.PlayerIds[0];

    public readonly TokenPlatform Platform;
    public readonly TokenGame Game;

    public DateTimeOffset LastContact;

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

    public bool IsExpired => DateTimeOffset.Now > this.LastContact + TimeSpan.FromMinutes(1);

    [JsonProperty("State"), JsonConverter(typeof(StringEnumConverter))]
    public RoomState RoomState;
    [JsonProperty("Mood"), JsonConverter(typeof(StringEnumConverter))]
    public RoomMood RoomMood;
    [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
    public RoomSlotType LevelType = RoomSlotType.Pod;
    
    [JsonProperty] public int LevelId;
}