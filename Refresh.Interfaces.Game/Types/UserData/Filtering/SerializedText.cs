using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.UserData.Filtering;

#nullable disable

[XmlRoot("text")]
public class SerializedText
{
    [XmlAttribute("id")]
    public int Id { get; set; }
    
    [XmlText]
    public string Text { get; set; }
}