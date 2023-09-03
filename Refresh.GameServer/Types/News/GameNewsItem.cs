using System.Xml.Serialization;

namespace Refresh.GameServer.Types.News;

public class GameNewsItem
{
    [XmlElement("id")] public required long Id { get; set; }
    [XmlElement("subject")] public required string Subject { get; set; }
    [XmlElement("content")] public required GameNewsItemContent Content { get; set; }
}