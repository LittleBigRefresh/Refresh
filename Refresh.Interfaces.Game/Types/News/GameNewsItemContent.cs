using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.News;

[XmlRoot("")]
public class GameNewsItemContent
{
    [XmlElement("frame")] public required GameNewsFrame Frame { get; set; }
}