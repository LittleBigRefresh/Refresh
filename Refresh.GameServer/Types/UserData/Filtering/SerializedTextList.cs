using System.Xml.Serialization;

namespace Refresh.GameServer.Types.UserData.Filtering;

#nullable disable

[XmlRoot("textList")]
public class SerializedTextList
{
    [XmlAttribute("allOk")]
    public bool AllOk { get; set; }
    [XmlElement("text")]
    public List<SerializedText> Text { get; set; }
}