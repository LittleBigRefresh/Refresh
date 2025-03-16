using System.Xml.Serialization;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Lists;

[XmlRoot("results")]
[XmlType("results")]
public class SerializedMinimalLevelResultsList : SerializedMinimalLevelList
{
    public SerializedMinimalLevelResultsList() {}
    
    public SerializedMinimalLevelResultsList(IEnumerable<SerializedMinimalLevelResponse>? list, int total, int skip)
    {
        this.Total = total;
        this.Items = list?.ToList() ?? [];
        this.NextPageStart = skip + 1;
    }
}