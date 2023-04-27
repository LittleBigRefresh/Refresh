using Newtonsoft.Json;

namespace Refresh.GameServer.Types.Legacy;

public class LegacyGameLocation
{
    [JsonProperty("x")] public int X { get; set; }
    [JsonProperty("y")] public int Y { get; set; }

    public static LegacyGameLocation FromGameLocation(GameLocation original) =>
        new()
        {
            X = original.X,
            Y = original.Y,
        };
}