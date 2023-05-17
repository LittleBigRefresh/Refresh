using Newtonsoft.Json;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Legacy;

#nullable disable

[JsonObject(MemberSerialization.OptIn)]
public class LegacyGameLevel
{
    [JsonProperty("slotId")] public int SlotId { get; set; }
    [JsonProperty("internalSlotId")] public int InternalSlotId { get; set; }
    [JsonProperty("type")] public byte Type { get; set; }
    [JsonProperty("name")] public string Name { get; set; }
    [JsonProperty("description")] public string Description { get; set; }
    [JsonProperty("location")] public LegacyGameLocation Location { get; set; }


    public static LegacyGameLevel FromGameLevel(GameLevel original) =>
        new()
        {
            SlotId = original.LevelId,
            InternalSlotId = original.LevelId,
            Name = original.Title,
            Description = original.Description,
            Location = LegacyGameLocation.FromGameLocation(original.Location),
            Type = 1,
        };

}