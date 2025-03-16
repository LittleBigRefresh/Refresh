using System.Xml.Serialization;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Lists;

#nullable disable

[XmlRoot("slots")]
[XmlType("slots")]
public class SerializedMinimalLevelList : SerializedList<SerializedMinimalLevelResponse>
{
    public SerializedMinimalLevelList() {}
    
    public SerializedMinimalLevelList(IEnumerable<SerializedMinimalLevelResponse> list, int total, int skip)
    {
        this.Total = total;
        this.Items = list.ToList();
        this.NextPageStart = skip + 1;
    }

    [XmlElement("slot")]
    public override List<SerializedMinimalLevelResponse> Items { get; set; }
}