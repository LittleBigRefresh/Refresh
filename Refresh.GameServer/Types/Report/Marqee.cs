using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.Report;

#nullable disable

[XmlRoot("marqee")]
public partial class Marqee
{ 
    [XmlElement("rect")] 
    public Rect Rect { get; set; } 
}