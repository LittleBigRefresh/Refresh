using System.Xml.Serialization;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Lists;

#nullable disable

[XmlRoot("myFriends")]
[XmlType("myFriends")]
public class SerializedFriendsList : SerializedList<GameUser>
{
    public SerializedFriendsList(List<GameUser> items)
    {
        this.Items = items;
    }

    public SerializedFriendsList() {}

    [XmlElement("user")]
    public sealed override List<GameUser> Items { get; set; }
}