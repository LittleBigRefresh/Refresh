using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.News;

public class GameNewsFrame
{
    [XmlAttribute("width")] public required long Width { get; set; }
    [XmlElement("title")] public required string Title { get; set; }
    [XmlElement("item")] public required GameNewsFrameItem Item { get; set; }
}