using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types;

[XmlType("location")]
public class GameLocation : EmbeddedObject
{
    [XmlElement("y")]
    public int X { get; set; }
    [XmlElement("x")]
    public int Y { get; set; }
}