using System.Xml.Serialization;
using Realms;

namespace Refresh.GameServer.Types;

[XmlType("location")]
public partial class GameLocation : IEmbeddedObject
{
    public static GameLocation Zero => new()
    {
        X = 0,
        Y = 0,
    };

    public GameLocation()
    {}

    public GameLocation(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    [XmlElement("y")] public int X { get; set; }
    [XmlElement("x")] public int Y { get; set; }
}