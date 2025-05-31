using System.Xml.Serialization;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Types.Data;

namespace Refresh.Database.Models.Activity.SerializedEvents;

[XmlType("event")]
[XmlRoot("event")]
[XmlInclude(typeof(SerializedUserEvent))]
[XmlInclude(typeof(SerializedLevelEvent))]
[XmlInclude(typeof(SerializedLevelUploadEvent))]
[XmlInclude(typeof(SerializedLevelPlayEvent))]
[XmlInclude(typeof(SerializedScoreSubmitEvent))]
[XmlInclude(typeof(SerializedPhotoUploadEvent))]
public abstract class SerializedEvent : IDataConvertableFrom<SerializedEvent, Event>
{
    [XmlAttribute("type")]
    public EventType Type { get; set; }
    [XmlElement("timestamp")]
    public long Timestamp { get; set; }
    [XmlElement("actor")]
    public string Actor { get; set; } = string.Empty;

    public static SerializedEvent? FromOld(Event? old, DataContext dataContext)
    {
        return null; // TODO
    }

    public static IEnumerable<SerializedEvent> FromOldList(IEnumerable<Event> oldList, DataContext dataContext)
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}