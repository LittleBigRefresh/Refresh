using System.Xml.Serialization;

namespace Refresh.GameServer.Types.News;

public class GameNewsFrameItemSlot
{
    [XmlElement("id")] 
    public required long Id { get; set; } 

    [XmlAttribute("type")] 
    public required string Type { get; set; } 
}