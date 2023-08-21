using System.Xml.Serialization;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;

namespace Refresh.GameServer.Types.Lists;

#nullable disable

[XmlRoot("slots")]
[XmlType("slots")]
public class SerializedLevelList : SerializedList<GameLevelResponse>
{
    [XmlElement("slot")]
    public override List<GameLevelResponse> Items { get; set; }
}