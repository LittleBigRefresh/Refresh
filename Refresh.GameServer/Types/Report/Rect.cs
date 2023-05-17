using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.Report;

[XmlRoot("rect")]
public partial class Rect : IEmbeddedObject 
{ 
    [XmlAttribute(AttributeName="t")] 
    public int Top { get; set; } 

    [XmlAttribute(AttributeName="l")] 
    public int Left { get; set; } 

    [XmlAttribute(AttributeName="b")] 
    public int Bottom { get; set; } 

    [XmlAttribute(AttributeName="r")] 
    public int Right { get; set; } 
}