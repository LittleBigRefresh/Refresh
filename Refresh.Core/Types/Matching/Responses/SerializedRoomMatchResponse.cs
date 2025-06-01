namespace Refresh.GameServer.Types.Matching.Responses;

#nullable disable

[JsonObject(MemberSerialization.OptIn)]
public class SerializedRoomMatchResponse
{
    [JsonProperty] public List<SerializedRoomPlayer> Players { get; set; }
    [JsonProperty] public List<List<int>> Slots { get; set; } // driving me insane
    [JsonProperty] public byte RoomState { get; set; }
    [JsonProperty] public byte HostMood { get; set; }
}