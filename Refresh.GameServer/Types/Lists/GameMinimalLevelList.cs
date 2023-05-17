using System.Xml.Serialization;
using JetBrains.Annotations;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Lists;

#nullable disable

[XmlRoot("slots")]
[XmlType("slots")]
public class GameMinimalLevelList : GameList<GameMinimalLevel>
{
    public GameMinimalLevelList() {}
    
    public GameMinimalLevelList(IEnumerable<GameMinimalLevel> list, int total)
    {
        this.Total = total;
        this.Items = list.ToList();
    }

    [XmlElement("slot")]
    public override List<GameMinimalLevel> Items { get; set; }
}