using System.Xml.Serialization;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Lists;

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
}