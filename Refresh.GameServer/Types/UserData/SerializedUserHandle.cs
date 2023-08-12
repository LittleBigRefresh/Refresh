using System.Xml.Serialization;

namespace Refresh.GameServer.Types.UserData;

[XmlRoot("npHandle")]
[XmlType("npHandle")]
public class SerializedUserHandle
{
    [XmlText] public string Username { get; set; } = string.Empty;

    [XmlAttribute("icon")] public string IconHash { get; set; } = "0";

    public static SerializedUserHandle FromUser(GameUser user) =>
        new()
        {
            Username = user.Username,
            IconHash = user.IconHash,
        };
}