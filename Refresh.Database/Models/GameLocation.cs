using System.Xml.Serialization;

namespace Refresh.Database.Models;

[XmlType("location")]
public class GameLocation
{
    public static GameLocation Zero => new()
    {
        X = 0,
        Y = 0,
    };

    public static GameLocation Random => new()
    {
        X = System.Random.Shared.Next(ushort.MaxValue),
        Y = System.Random.Shared.Next(ushort.MaxValue),
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