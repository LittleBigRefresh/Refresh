using Newtonsoft.Json;

namespace Refresh.GameServer.Types.Legacy;

public class LegacyStatus
{
    [JsonProperty("statusType")] public int StatusType { get; set; } = 1;
    [JsonProperty("currentVersion")] public int? CurrentVersion { get; set; } = 1;
    [JsonProperty("currentPlatform")] public int? CurrentPlatform { get; set; } = 0;
    [JsonProperty("currentRoom")] public LegacyRoom? CurrentRoom { get; set; } = new();
    [JsonProperty("lastLogin")] public int LastLogin { get; set; }
    [JsonProperty("lastLogout")] public int LastLogout { get; set; }
}