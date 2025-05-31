using System.Xml.Serialization;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;

namespace Refresh.GameServer.Types.Lists;

[XmlRoot("users")]
public class SerializedUserList
{
    [XmlElement("user")]
    public IEnumerable<GameUserResponse> Users { get; set; } = [];
}