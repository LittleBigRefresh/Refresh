using System.Xml.Serialization;
using Refresh.GameServer.Types.Levels.Categories;

namespace Refresh.GameServer.Types.Lists;
[XmlRoot("categories")]
[XmlType("categories")]
public class SerializedCategoryList : SerializedList<SerializedCategory>
{
    public SerializedCategoryList(IEnumerable<SerializedCategory> items, CategoryService categories)
    {
        this.Items = items.ToList();
        this.Total = categories.Categories.Count();
    }

    public SerializedCategoryList() {}

    [XmlElement("category")]
    public sealed override List<SerializedCategory> Items { get; set; } = null!;

    [XmlAttribute("hint")]
    public string Hint { get; set; } = string.Empty;
}