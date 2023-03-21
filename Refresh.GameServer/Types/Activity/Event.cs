using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Activity;

#nullable disable

/// <summary>
/// An action performed by a user.
/// </summary>
[Serializable]
public class Event : RealmObject
{
    /// <summary>
    /// The ID of the event.
    /// </summary>
    public ObjectId EventId { get; set; } = ObjectId.GenerateNewId();
    
    [Ignored]
    public EventType EventType
    {
        get => (EventType)this._EventType;
        set => this._EventType = (int)value;
    }
    
    // ReSharper disable once InconsistentNaming
    internal int _EventType { get; set; }

    /// <summary>
    /// The user in question that created this event.
    /// </summary>
    public GameUser User { get; set; }
    
    /// <summary>
    /// Should this event be shown to other users on the server?
    /// </summary>
    public bool IsPrivate { get; set; }
    
    /// <summary>
    /// The type of data that this event is referencing.
    /// </summary>
    [Ignored]
    public EventDataType StoredDataType
    {
        get => (EventDataType)this._StoredDataType;
        set => this._StoredDataType = (int)value;
    }
    
    // ReSharper disable once InconsistentNaming
    internal int _StoredDataType { get; set; }
    
    /// <summary>
    /// The sequential ID of the object this event is referencing. If null, use <see cref="StoredObjectId"/>.
    /// </summary>
    internal int? StoredSequentialId { get; set; }
    
    /// <summary>
    /// The ObjectId of the object this event is referencing. If null, use <see cref="StoredSequentialId"/>.
    /// </summary>
    internal ObjectId? StoredObjectId { get; set; }
}