using System.Xml.Serialization;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Lists;

[XmlRoot("results")]
[XmlType("results")]
public class SerializedMinimalLevelResultsList : SerializedMinimalLevelList
{
    public SerializedMinimalLevelResultsList() {}
    
    public SerializedMinimalLevelResultsList(IEnumerable<GameMinimalLevel>? list, int total)
    {
        this.Total = total;
        this.Items = list?.ToList() ?? new List<GameMinimalLevel>();
    }
}