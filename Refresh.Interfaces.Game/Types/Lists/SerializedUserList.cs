using System.Xml.Serialization;
using Refresh.Interfaces.Game.Endpoints.DataTypes.Response;

namespace Refresh.Interfaces.Game.Types.Lists;

[XmlRoot("users")]
public class SerializedUserList
{
    [XmlElement("user")]
    public List<GameUserResponse> Users { get; set; } = [];
}