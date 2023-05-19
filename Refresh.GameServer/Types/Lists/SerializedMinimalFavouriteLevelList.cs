using System.Xml.Serialization;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Lists;

#nullable disable

[XmlRoot("favouriteSlots")]
[XmlType("favouriteSlots")]
public class SerializedMinimalFavouriteLevelList : SerializedList<GameMinimalLevel>
{
    public SerializedMinimalFavouriteLevelList() {}
    
    public SerializedMinimalFavouriteLevelList(SerializedMinimalLevelList list)
    {
        this.Total = list.Total;
        this.Items = list.Items;
    }
    
    [XmlElement("slot")]
    public override List<GameMinimalLevel> Items { get; set; }
}