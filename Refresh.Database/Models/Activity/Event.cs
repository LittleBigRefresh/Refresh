using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using MongoDB.Bson;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Activity;

#nullable disable

/// <summary>
/// An action performed by a user.
/// </summary>
[Serializable]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[JsonObject(MemberSerialization.OptOut, ItemNullValueHandling = NullValueHandling.Ignore)]
[XmlRoot("event")]
[XmlType("event")]
public partial class Event : IRealmObject
{
    /// <summary>
    /// The ID of the event.
    /// </summary>
    [XmlIgnore]
    public ObjectId EventId { get; set; } = ObjectId.GenerateNewId();
    
    [Ignored, NotMapped]
    [XmlAttribute("type")]
    public EventType EventType
    {
        get => (EventType)this._EventType;
        set => this._EventType = (int)value;
    }
    
    // ReSharper disable once InconsistentNaming
    public int _EventType { get; set; }

    /// <summary>
    /// The user in question that created this event.
    /// </summary>
    [XmlIgnore] [JsonIgnore] public GameUser User { get; set; }

    [XmlIgnore] public string UserId => this.User.UserId.ToString(); 
    
    /// <summary>
    /// Should this event be shown to other users on the server?
    /// </summary>
    [XmlIgnore]
    [JsonIgnore]
    public bool IsPrivate { get; set; }
    
    [XmlElement("timestamp")]
    public DateTimeOffset Timestamp { get; set; }
    
    /// <summary>
    /// The type of data that this event is referencing.
    /// </summary>
    [Ignored, NotMapped]
    [XmlIgnore]
    public EventDataType StoredDataType
    {
        get => (EventDataType)this._StoredDataType;
        set => this._StoredDataType = (int)value;
    }
    
    // ReSharper disable once InconsistentNaming
    public int _StoredDataType { get; set; }
    
    /// <summary>
    /// The sequential ID of the object this event is referencing. If null, use <see cref="StoredObjectId"/>.
    /// </summary>
    [XmlIgnore]
    public int? StoredSequentialId { get; set; }
    
    /// <summary>
    /// The ObjectId of the object this event is referencing. If null, use <see cref="StoredSequentialId"/>.
    /// </summary>
    [XmlIgnore]
    public ObjectId? StoredObjectId { get; set; }
}