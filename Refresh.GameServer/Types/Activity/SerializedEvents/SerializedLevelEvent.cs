using System.Xml.Serialization;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Activity.SerializedEvents;

public class SerializedLevelEvent : SerializedEvent
{
    [XmlElement("object_slot_id")] public GameLevelId LevelId { get; set; } = new();
}