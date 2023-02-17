using System.Xml.Serialization;
using NotEnoughLogs.Definitions;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Lists;

#nullable disable

[XmlRoot("favouriteSlots")]
[XmlType("favouriteSlots")]
public class GameMinimalHeartedLevelList : GameList<GameMinimalLevel>
{
    public GameMinimalHeartedLevelList() {}
    
    public GameMinimalHeartedLevelList(GameMinimalLevelList list)
    {
        this.Total = list.Total;
        this.Items = list.Items;
    }
    
    [XmlElement("slot")]
    public override List<GameMinimalLevel> Items { get; set; }
}