using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Activity;

public enum EventType
{
    [XmlEnum("level_upload")]
    LevelUpload,
}