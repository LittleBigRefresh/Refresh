using System.Xml.Serialization;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Lists.Results;

[XmlRoot("results")]
[XmlType("results")]
public class SerializedMinimalLevelResultsList : SerializedMinimalLevelList
{
    public SerializedMinimalLevelResultsList() {}
    
    public SerializedMinimalLevelResultsList(IEnumerable<GameMinimalLevelResponse>? list, int? nextPageIndex, int? totalItems)
    {
        this.Items = list?.ToList() ?? [];
        this.NextPageStart = nextPageIndex ?? -1;
        this.Total = totalItems ?? 0;
    }
}