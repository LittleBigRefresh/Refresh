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
[Index(nameof(Timestamp))]
public partial class Event
{
    /// <summary>
    /// The ID of the event.
    /// </summary>
    [Key] public ObjectId EventId { get; set; } = ObjectId.GenerateNewId();
    
    public EventType EventType { get; set; }

    /// <summary>
    /// The user in question that created this event, the "actor".
    /// </summary>
    [Required, ForeignKey(nameof(UserId))] public GameUser User { get; set; }
    [Required] public ObjectId UserId { get; set; }

    #nullable restore

    /// <summary>
    /// A reference to the user "involved" in this event. They may also see this event even if it's private.
    /// Usually the object publisher/owner (eg. level/photo publisher).
    /// </summary>
    [ForeignKey(nameof(InvolvedUserId))] public GameUser? InvolvedUser { get; set; }
    public ObjectId? InvolvedUserId { get; set; }

    #nullable disable
    
    /// <summary>
    /// Whether this event is for activity, moderation etc.
    /// Server should use this to determine who this event is visible to.
    /// </summary>
    public EventOverType OverType { get; set; }
    
    [XmlElement("timestamp")]
    public DateTimeOffset Timestamp { get; set; }
    
    /// <summary>
    /// The type of data that this event is referencing.
    /// </summary>
    public EventDataType StoredDataType { get; set; }

    // TODO: Find out and decide how to also store string IDs which are not object IDs (e.g. GameAsset hash or Contest ID)
    
    /// <summary>
    /// The sequential ID of the object this event is referencing. If null, use <see cref="StoredObjectId"/>.
    /// </summary>
    public int? StoredSequentialId { get; set; }
    
    /// <summary>
    /// The ObjectId of the object this event is referencing. If null, use <see cref="StoredSequentialId"/>.
    /// </summary>
    public ObjectId? StoredObjectId { get; set; }

    /// <summary>
    /// An additional description of this event. Useful if this event is a moderation action (to store the reason), for example.
    /// </summary>
    public string AdditionalInfo { get; set; } = "";

    /// <summary>
    /// Can be used by various events to indicate whether content has been initially created or edited 
    /// (can show this for level and review upload events in-game, for instance, or for other UGC edited by staff on the API).
    /// </summary>
    public bool IsModified { get; set; }
}