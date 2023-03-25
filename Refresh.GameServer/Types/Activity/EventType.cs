using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Activity;

public enum EventType
{
    [XmlEnum("publish_level")]
    LevelUpload,
    [XmlEnum("heart_level")]
    LevelFavourite,
    [XmlEnum("unheart_level")]
    LevelUnfavourite,
}