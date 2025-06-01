using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.News;

public class GameNewsSubcategory
{
    [XmlElement("id")] public required long Id { get; set; }
    [XmlElement("title")] public required string Title { get; set; }
    [XmlElement("item")] public required List<GameNewsItem> Items { get; set; }
}