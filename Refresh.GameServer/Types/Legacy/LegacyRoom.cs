namespace Refresh.GameServer.Types.Legacy;

public class LegacyRoom
{
    [JsonProperty("roomId")] public int RoomId { get; set; }
    [JsonProperty("playerIds")] public int[]? PlayerIds { get; set; } = Array.Empty<int>();
    [JsonProperty("slot")] public LegacyRoomSlot? Slot { get; set; } = new();
}

public class LegacyRoomSlot
{
    [JsonProperty("slotId")] public int SlotId { get; set; } = 0;
    [JsonProperty("slotType")] public int SlotType { get; set; } = 5;
}
