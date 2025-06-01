using System.Xml.Serialization;
using Refresh.Interfaces.Game.Endpoints.DataTypes.Response;

namespace Refresh.Interfaces.Game.Types.Lists;

#nullable disable

[XmlRoot("slots")]
[XmlType("slots")]
public class SerializedLevelList : SerializedList<GameLevelResponse>
{
    [XmlElement("slot")]
    public override List<GameLevelResponse> Items { get; set; }
}