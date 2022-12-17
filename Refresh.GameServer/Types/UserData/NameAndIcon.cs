using System.Xml.Serialization;

namespace Refresh.GameServer.Types.UserData;

[XmlRoot("npHandle")]
[XmlType("npHandle")]
public class NameAndIcon
{
    [XmlText] public string Username { get; set; } = string.Empty;

    [XmlAttribute("icon")] public string IconHash { get; set; } = "0";
}