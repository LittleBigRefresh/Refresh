using System.Xml.Serialization;

namespace Refresh.GameServer.Types;

[XmlType("location")]
public class GameLocation
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

    public static GameLocation RandomLocation() 
    {
        Random random = new();
        
        return new GameLocation
        (
            random.Next(65535), 
            random.Next(65535)
        );
    }
}