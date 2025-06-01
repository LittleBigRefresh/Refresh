using System.Xml.Serialization;
using Refresh.Interfaces.Game.Types.Levels;

namespace Refresh.Interfaces.Game.Types.Lists;

#nullable disable

[XmlRoot("favouriteSlots")]
[XmlType("favouriteSlots")]
public class SerializedMinimalFavouriteLevelList : SerializedList<GameMinimalLevelResponse>
{
    public SerializedMinimalFavouriteLevelList() {}
    
    public SerializedMinimalFavouriteLevelList(SerializedMinimalLevelList list)
    {
        this.Total = list.Total;
        this.NextPageStart = list.NextPageStart;
        this.Items = list.Items;
    }
    
    [XmlElement("slot")]
    public override List<GameMinimalLevelResponse> Items { get; set; }
}