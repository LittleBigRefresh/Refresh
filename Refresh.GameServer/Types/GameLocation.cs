using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types;

[XmlType("location")]
public class GameLocation : EmbeddedObject
{
    public static readonly GameLocation Zero = new()
    {
        X = 0,
        Y = 0,
    };
    
    [XmlElement("y")]
    public int X { get; set; }
    [XmlElement("x")]
    public int Y { get; set; }
}