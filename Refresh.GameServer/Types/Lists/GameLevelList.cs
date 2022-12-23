using System.Xml.Serialization;
using NotEnoughLogs.Definitions;

namespace Refresh.GameServer.Types.Lists;

#nullable disable

[XmlRoot("slots")]
[XmlType("slots")]
public class GameLevelList : GameList<Level>
{
    [XmlElement("slot")]
    public override List<Level> Items { get; set; }
}