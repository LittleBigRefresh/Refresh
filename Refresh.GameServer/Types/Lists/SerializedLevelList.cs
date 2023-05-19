using System.Xml.Serialization;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Lists;

#nullable disable

[XmlRoot("slots")]
[XmlType("slots")]
public class SerializedLevelList : SerializedList<GameLevel>
{
    [XmlElement("slot")]
    public override List<GameLevel> Items { get; set; }
}