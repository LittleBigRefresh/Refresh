using System.Xml.Serialization;

namespace Refresh.GameServer.Types.News;

public class GameNewsSubcategory
{
    [XmlElement("id")] public required long Id { get; set; }
    [XmlElement("title")] public required string Title { get; set; }
    [XmlElement("item")] public required GameNewsItem Item { get; set; }
}