using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Categories;

#nullable disable

[XmlType("category")]
public class SerializedCategory
{
    [XmlElement("name")] public string Name { get; set; }
    [XmlElement("description")] public string Description { get; set; }
    [XmlElement("url")] public string Url { get; set; }
    [XmlElement("tag")] public string Tag { get; set; }
    [XmlElement("icon")] public string IconHash { get; set; }
}