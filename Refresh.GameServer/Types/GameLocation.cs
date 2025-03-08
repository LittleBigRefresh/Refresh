using System.Xml.Serialization;

namespace Refresh.GameServer.Types;

[XmlType("location")]
public class GameLocation
{
    public static GameLocation ZeroLocation => new()
    {
        X = 0,
        Y = 0,
    };

    public static GameLocation RandomLocation => new()
    {
        X = Random.Shared.Next(ushort.MaxValue),
        Y = Random.Shared.Next(ushort.MaxValue),
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