using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Legacy;

#nullable disable

public class LegacyGameUser : IDataConvertableFrom<LegacyGameUser, GameUser>
{
    [JsonProperty("userId")] public required int UserId { get; set; }
    [JsonProperty("username")] public required string Username { get; set; }
    [JsonProperty("emailAddressVerified")] public required bool EmailAddressVerified { get; set; }
    [JsonProperty("iconHash")] public required string IconHash { get; set; }
    [JsonProperty("biography")] public required string Biography { get; set; }
    [JsonProperty("location")] public required LegacyGameLocation Location { get; set; }
    [JsonProperty("yayHash")] public required string YayHash { get; set; }
    [JsonProperty("mehHash")] public required string MehHash { get; set; }
    [JsonProperty("booHash")] public required string BooHash { get; set; }
    [JsonProperty("lastLogin")] public required long LastLogin { get; set; }
    [JsonProperty("lastLogout")] public required long LastLogout { get; set; }
    [JsonProperty("levelVisibility")] public required short LevelVisibility { get; set; }
    [JsonProperty("profileVisibility")] public required short ProfileVisibility { get; set; }
    [JsonProperty("commentsEnabled")] public required bool CommentsEnabled { get; set; }
    [JsonProperty("permissionLevel")] public required int PermissionLevel { get; set; }

    public static LegacyGameUser FromOld(GameUser original)
    {
        return new LegacyGameUser
        {
            UserId = original.UserId.Timestamp,
            Username = original.Username,
            Biography = original.Description,
            Location = LegacyGameLocation.FromGameLocation(original.Location),
            LastLogin = original.JoinDate.ToUnixTimeMilliseconds(),
            LastLogout = original.JoinDate.ToUnixTimeMilliseconds(),
            EmailAddressVerified = true,
            IconHash = "0",
            YayHash = "0",
            MehHash = "0",
            BooHash = "0",
            LevelVisibility = 2,
            ProfileVisibility = 2,
            CommentsEnabled = true,
            PermissionLevel = 0, // we don't expose this information for security reasons
        };
    }

    public static IEnumerable<LegacyGameUser> FromOldList(IEnumerable<GameUser> oldList) => oldList.Select(FromOld)!;
}