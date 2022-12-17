using System.Xml.Serialization;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Lists;

[XmlRoot("users")]
public class GameUserList
{
    [XmlElement("user")]
    public List<GameUser> Users { get; set; } = new();
}