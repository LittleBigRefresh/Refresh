using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.UserData;

[XmlRoot("updateUser")]
[Ignored]
public class UpdateUserData
{
    [XmlElement("biography")]
    public string? Description { get; set; }
    [XmlElement("location")]
    public GameLocation? Location { get; set; }
}