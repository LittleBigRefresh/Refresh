using System.Xml.Serialization;
using Realms;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels;

public class GameMinimalLevel
{
    [XmlElement("name")] public string Title { get; set; } = string.Empty;
    [XmlElement("icon")] public string IconHash { get; set; } = string.Empty;
    [XmlElement("description")] public string Description { get; set; } = string.Empty;
    [XmlElement("location")] public GameLocation Location { get; set; } = GameLocation.Zero;
    [XmlElement("id")] public int SerializationId { get; set; }
    [XmlElement("npHandle")] public NameAndIcon? Handle { get; set; }
    [XmlAttribute("type")] public string? Type { get; set; }
    
    private GameMinimalLevel() {}

    public static GameMinimalLevel FromGameLevel(GameLevel level)
    {
        level.PrepareForSerialization();
        return new GameMinimalLevel
        {
            Title = level.Title,
            IconHash = level.IconHash,
            Description = level.Description,
            Location = level.Location,
            SerializationId = level.SerializationId,
            Handle = level.Handle,
            Type = level.Type,
        };
    }
}