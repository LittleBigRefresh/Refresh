using Newtonsoft.Json;

namespace Refresh.GameServer.Types.Legacy;

public class LegacyRoom
{
    [JsonProperty("roomId")] public int RoomId { get; set; }
    [JsonProperty("playerIds")] public int[]? PlayerIds { get; set; } = { 1676779928 };
    [JsonProperty("slot")] public LegacyRoomSlot? Slot { get; set; } = new();
}

public class LegacyRoomSlot
{
    [JsonProperty("slotId")]
    public int SlotId { get; set; }

    [JsonProperty("slotType")]
    public int SlotType { get; set; }
}
