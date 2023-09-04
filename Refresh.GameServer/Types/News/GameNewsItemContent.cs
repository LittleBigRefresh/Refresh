using System.Xml.Serialization;

namespace Refresh.GameServer.Types.News;

public class GameNewsItemContent
{
    [XmlElement("frame")] public required GameNewsFrame Frame { get; set; }
}