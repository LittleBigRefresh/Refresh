using System.Xml.Serialization;
using Refresh.GameServer.Types.Levels;

namespace Refresh.Database.Models.Activity.SerializedEvents;

public class SerializedLevelEvent : SerializedEvent
{
    [XmlElement("object_slot_id")] public SerializedLevelId LevelId { get; set; } = new();
}