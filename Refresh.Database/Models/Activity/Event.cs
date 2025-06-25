using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using MongoDB.Bson;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Activity;

#nullable disable

/// <summary>
/// An action performed by a user.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public partial class Event
{
    /// <summary>
    /// The ID of the event.
    /// </summary>
    public ObjectId EventId { get; set; } = ObjectId.GenerateNewId();
    
    public EventType EventType { get; set; }


    /// <summary>
    /// The user in question that created this event.
    /// </summary>
    [Required]
    public GameUser User { get; set; }
    
    /// <summary>
    /// Should this event be shown to other users on the server?
    /// </summary>
    public bool IsPrivate { get; set; }
    
    [XmlElement("timestamp")]
    public DateTimeOffset Timestamp { get; set; }
    
    /// <summary>
    /// The type of data that this event is referencing.
    /// </summary>
    public EventDataType StoredDataType { get; set; }
    
    /// <summary>
    /// The sequential ID of the object this event is referencing. If null, use <see cref="StoredObjectId"/>.
    /// </summary>
    public int? StoredSequentialId { get; set; }
    
    /// <summary>
    /// The ObjectId of the object this event is referencing. If null, use <see cref="StoredSequentialId"/>.
    /// </summary>
    public ObjectId? StoredObjectId { get; set; }
}