using System.Xml.Serialization;
using Refresh.GameServer.Types.Levels.Categories;

namespace Refresh.GameServer.Types.Lists;
[XmlRoot("categories")]
[XmlType("categories")]
public class SerializedCategoryList : SerializedList<SerializedCategory>
{
    public SerializedCategoryList(IEnumerable<SerializedCategory> items, SearchLevelCategory searchCategory, int total)
    {
        this.Items = items.ToList();
        this.TextSearchCategory = SerializedCategory.FromLevelCategory(searchCategory);
        this.Total = total;
    }

    public SerializedCategoryList() {}

    [XmlElement("category")]
    public sealed override List<SerializedCategory> Items { get; set; } = null!;

    [XmlElement("text_search")]
    public SerializedCategory TextSearchCategory { get; set; } = null!;

    [XmlAttribute("hint")]
    public string Hint { get; set; } = string.Empty;
}