using System.Xml.Serialization;
using Refresh.Database.Models.Users;

namespace Refresh.GameServer.Types.News;

public class GameNewsFrameItem
{
    [XmlAttribute("width")] public required long Width { get; set; }
    [XmlElement("slot")] public GameNewsFrameItemSlot? Level { get; set; }
    [XmlElement("npHandle")] public SerializedUserHandle? Handle { get; set; }
    [XmlElement("content")] public required string? Content { get; set; }
    [XmlElement("background")] public required string Background { get; set; }
}