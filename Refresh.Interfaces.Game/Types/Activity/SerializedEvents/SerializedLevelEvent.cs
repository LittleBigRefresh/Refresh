using System.Xml.Serialization;
using Refresh.Interfaces.Game.Types.Levels;

namespace Refresh.Interfaces.Game.Types.Activity.SerializedEvents;

public class SerializedLevelEvent : SerializedEvent
{
    [XmlElement("object_slot_id")] public SerializedLevelId LevelId { get; set; } = new();
}