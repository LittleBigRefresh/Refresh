using System.Xml.Serialization;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;

namespace Refresh.GameServer.Types.Lists;

[XmlRoot("users")]
public class SerializedUserList
{
    [XmlElement("user")]
    public List<GameUserResponse> Users { get; set; } = new();
}