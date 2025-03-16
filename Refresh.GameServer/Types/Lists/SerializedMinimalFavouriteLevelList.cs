using System.Xml.Serialization;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Lists;

#nullable disable

[XmlRoot("favouriteSlots")]
[XmlType("favouriteSlots")]
public class SerializedMinimalFavouriteLevelList : SerializedList<SerializedMinimalLevelResponse>
{
    public SerializedMinimalFavouriteLevelList() {}
    
    public SerializedMinimalFavouriteLevelList(SerializedMinimalLevelList list)
    {
        this.Total = list.Total;
        this.NextPageStart = list.NextPageStart;
        this.Items = list.Items;
    }
    
    [XmlElement("slot")]
    public override List<SerializedMinimalLevelResponse> Items { get; set; }
}