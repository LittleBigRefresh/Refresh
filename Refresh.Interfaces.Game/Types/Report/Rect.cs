using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Report;

[XmlRoot("rect")]
public class Rect
{ 
    [XmlAttribute(AttributeName="t")] 
    public long Top { get; set; } 

    [XmlAttribute(AttributeName="l")] 
    public long Left { get; set; } 

    [XmlAttribute(AttributeName="b")] 
    public long Bottom { get; set; } 

    [XmlAttribute(AttributeName="r")] 
    public long Right { get; set; } 
}