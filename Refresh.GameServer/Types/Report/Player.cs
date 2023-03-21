using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types.Report;

[XmlRoot(ElementName="player")]
public class Player : EmbeddedObject 
{ 
    [XmlElement(ElementName="id")] 
    public string Username { get; set; } 

    [XmlElement(ElementName="rect")] 
    public Rect Rect { get; set; } 

    [XmlAttribute(AttributeName="reporter")] 
    public bool Reporter { get; set; } 

    [XmlAttribute(AttributeName="ingamenow")] 
    public bool Ingamenow { get; set; } 

    [XmlAttribute(AttributeName="playerNumber")] 
    public int PlayerNumber { get; set; } 

    [XmlText] 
    public string Text { get; set; } 

    [XmlElement(ElementName="screenRect")] 
    public ScreenRect ScreenRect { get; set; } 
}