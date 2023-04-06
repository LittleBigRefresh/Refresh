using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.Report;

#nullable disable

[XmlRoot("player")]
public class Player : EmbeddedObject 
{ 
    [XmlElement("id")] 
    public string Username { get; set; } 

    [XmlElement("rect")] 
    public Rect Rectangle { get; set; } 

    [XmlAttribute("reporter")] 
    public bool Reporter { get; set; } 

    [XmlAttribute("ingamenow")] 
    public bool IngameNow { get; set; } 

    [XmlAttribute("playerNumber")] 
    public int PlayerNumber { get; set; } 

    [XmlText] 
    public string Text { get; set; } 

    [XmlElement("screenRect")] 
    public ScreenRect ScreenRect { get; set; } 
}