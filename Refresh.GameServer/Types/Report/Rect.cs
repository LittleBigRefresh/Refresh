using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.Report;

[XmlRoot(ElementName = "rect")]
public class Rect : EmbeddedObject 
{ 
    [XmlAttribute(AttributeName="t")] 
    public int T { get; set; } 

    [XmlAttribute(AttributeName="l")] 
    public int L { get; set; } 

    [XmlAttribute(AttributeName="b")] 
    public int B { get; set; } 

    [XmlAttribute(AttributeName="r")] 
    public int R { get; set; } 
}