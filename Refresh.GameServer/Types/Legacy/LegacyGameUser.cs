using Newtonsoft.Json;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Legacy;

#nullable disable

public class LegacyGameUser
{
    [JsonProperty("userId")] public int UserId { get; set; }
    [JsonProperty("username")] public string Username { get; set; }
    [JsonProperty("emailAddressVerified")] public bool EmailAddressVerified { get; set; } = true;
    [JsonProperty("iconHash")] public string IconHash { get; set; } = "0";
    [JsonProperty("biography")] public string Biography { get; set; }
    [JsonProperty("location")] public LegacyGameLocation Location { get; set; }
    [JsonProperty("yayHash")] public string YayHash { get; set; }
    [JsonProperty("mehHash")] public string MehHash { get; set; }
    [JsonProperty("booHash")] public string BooHash { get; set; }
    [JsonProperty("lastLogin")] public int LastLogin { get; set; } = 1;
    [JsonProperty("lastLogout")] public int LastLogout { get; set; }
    [JsonProperty("levelVisibility")] public short LevelVisibility { get; set; }
    [JsonProperty("profileVisibility")] public short ProfileVisibility { get; set; }
    [JsonProperty("commentsEnabled")] public bool CommentsEnabled { get; set; } = true;

    public static LegacyGameUser FromGameUser(GameUser original)
    {
        return new LegacyGameUser
        {
            UserId = original.UserId.Timestamp,
            Username = original.Username,
            Biography = original.Description,
            Location = LegacyGameLocation.FromGameLocation(original.Location),
        };
    }
}