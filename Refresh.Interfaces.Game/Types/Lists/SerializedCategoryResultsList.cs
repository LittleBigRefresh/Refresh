using System.Xml.Serialization;
using Refresh.Interfaces.Game.Endpoints.DataTypes.Response;
using Refresh.Interfaces.Game.Types.Levels;

namespace Refresh.Interfaces.Game.Types.Lists;

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

    public SerializedCategoryResultsList(IEnumerable<GameUserResponse> users, int nextPageIndex, int totalItems)
    {
        this.Users = users.ToList();
        this.NextPageStart = nextPageIndex!;
        this.Total = totalItems;
    }

    [XmlElement("slot")] public List<GameMinimalLevelResponse> Levels { get; set; } = [];
    [XmlElement("user")] public List<GameUserResponse> Users { get; set; } = [];
}