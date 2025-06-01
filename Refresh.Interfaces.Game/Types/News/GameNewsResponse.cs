using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.News;

[XmlRoot("news")]
public class GameNewsResponse
{
    [XmlElement("subcategory")]
    public required GameNewsSubcategory Subcategory { get; set; }
}