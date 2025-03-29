using System.Xml.Serialization;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;

namespace Refresh.GameServer.Types.Lists;

[XmlRoot("users")]
public class SerializedUserList : SerializedList<GameUserResponse>
{
    [XmlElement("user")]
    public override List<GameUserResponse> Items { get; set; } = [];
}