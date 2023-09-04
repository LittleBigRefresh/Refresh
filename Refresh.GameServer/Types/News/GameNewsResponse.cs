using System.Xml.Serialization;

namespace Refresh.GameServer.Types.News;

[XmlRoot("news")]
public class GameNewsResponse
{
    [XmlElement("subcategory")]
    public required GameNewsSubcategory Subcategory { get; set; }
}