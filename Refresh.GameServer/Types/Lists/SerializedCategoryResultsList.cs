using System.Xml.Serialization;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Lists;

[XmlRoot("results")]
[XmlType("results")]
public class SerializedCategoryResultsList : SerializedPaginationData
{
    public SerializedCategoryResultsList() {}

    public SerializedCategoryResultsList(IEnumerable<GameMinimalLevelResponse> levels, int nextPageIndex, int totalItems)
    {
        this.Levels = levels.ToList();
        this.NextPageStart = nextPageIndex!;
        this.Total = totalItems;
    }

    [XmlElement("slot")] public List<GameMinimalLevelResponse> Levels { get; set; } = [];
}