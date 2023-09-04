using System.Xml.Serialization;

namespace Refresh.GameServer.Types.UserData.Filtering;

#nullable disable

[XmlRoot("textList")]
public class SerializedTextList
{
    [XmlElement("text")]
    public List<SerializedText> Text { get; set; }
}