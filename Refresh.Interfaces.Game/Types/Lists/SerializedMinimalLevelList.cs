using System.Xml.Serialization;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Lists;

#nullable disable

[XmlRoot("slots")]
[XmlType("slots")]
public class SerializedMinimalLevelList : SerializedList<GameMinimalLevelResponse>
{
    public SerializedMinimalLevelList() {}
    
    public SerializedMinimalLevelList(IEnumerable<GameMinimalLevelResponse> list, int total, int skip)
    {
        this.Total = total;
        this.Items = list.ToList();
        this.NextPageStart = skip + 1;
    }

    [XmlElement("slot")]
    public override List<GameMinimalLevelResponse> Items { get; set; }
}