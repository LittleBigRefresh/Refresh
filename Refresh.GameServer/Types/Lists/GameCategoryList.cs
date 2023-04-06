using System.Xml.Serialization;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Levels.Categories;

namespace Refresh.GameServer.Types.Lists;
[XmlRoot("categories")]
[XmlType("categories")]
public class GameCategoryList : GameList<GameCategory>
{
    public GameCategoryList(IEnumerable<GameCategory> items)
    {
        this.Items = items.ToList();
        this.Total = CategoryHandler.Categories.Count();
    }

    public GameCategoryList() {}

    [XmlElement("category")]
    public sealed override List<GameCategory> Items { get; set; } = null!;

    [XmlAttribute("hint")]
    public string Hint { get; set; } = string.Empty;
}