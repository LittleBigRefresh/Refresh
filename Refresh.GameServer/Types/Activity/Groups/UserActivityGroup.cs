using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Activity.Groups;

#nullable disable

public class UserActivityGroup : ActivityGroup
{
    [XmlAttribute("type")]
    public override string Type { get; set; } = "user";

    [XmlElement("user_id")]
    public string Username { get; set; }
}