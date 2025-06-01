using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Activity.Groups;

#nullable disable

public class UserSerializedActivityGroup : SerializedActivityGroup
{
    [XmlAttribute("type")]
    public override string Type { get; set; } = "user";

    [XmlElement("user_id")]
    public string Username { get; set; }
}