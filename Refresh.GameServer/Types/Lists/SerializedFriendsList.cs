using System.Xml.Serialization;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;

namespace Refresh.GameServer.Types.Lists;

#nullable disable

[XmlRoot("myFriends")]
[XmlType("myFriends")]
public class SerializedFriendsList : SerializedList<GameUserResponse>
{
    public SerializedFriendsList(List<GameUserResponse> items)
    {
        this.Items = items;
    }

    public SerializedFriendsList() {}

    [XmlElement("user")]
    public sealed override List<GameUserResponse> Items { get; set; }
}