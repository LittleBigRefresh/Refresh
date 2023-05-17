using System.Xml.Serialization;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Lists;

#nullable disable

[XmlRoot("myFriends")]
[XmlType("myFriends")]
public class GameFriendsList : GameList<GameUser>
{
    public GameFriendsList(List<GameUser> items)
    {
        this.Items = items;
    }

    public GameFriendsList() {}

    [XmlElement("user")]
    public sealed override List<GameUser> Items { get; set; }
}