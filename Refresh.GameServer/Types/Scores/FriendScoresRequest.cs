using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Scores;

#nullable disable

[XmlRoot("playRecord")]
public class FriendScoresRequest
{
    [XmlElement("playerIds")]
    public List<string> Usernames { get; set; }
    
    [XmlElement("type")]
    public byte Type { get; set; }
}