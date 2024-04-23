using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.Report;

[XmlRoot("rect")]
public partial class Rect
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