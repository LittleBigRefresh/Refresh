using MongoDB.Bson;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Moderation;

#nullable disable

public partial class ModerationAction
{
    [Key] public ObjectId ActionId { get; set; } = ObjectId.GenerateNewId();
    
    /// <summary>
    /// Describes what was done with the object.
    /// </summary>
    public ModerationActionType ActionType { get; set; }

    /// <summary>
    /// The type of data/object that was moderated. 
    /// </summary>
    public ModerationObjectType ModeratedObjectType { get; set; }
    
    /// <summary>
    /// The ID of the object that was moderated. May be a UUID, sequential ID, or a GameAsset hash,
    /// depending on ModeratedObjectType's value
    /// </summary>
    [Required] public string ModeratedObjectId { get; set; }

    /// <summary>
    /// The user in question who has moderated the object.
    /// </summary>
    [Required, ForeignKey(nameof(ActorId))] public GameUser Actor { get; set; }
    [Required] public ObjectId ActorId { get; set; }

    #nullable restore

    /// <summary>
    /// Usually the publisher/owner of the object that was moderated. May also see this moderation action.
    /// </summary>
    [ForeignKey(nameof(InvolvedUserId))] public GameUser? InvolvedUser { get; set; }
    public ObjectId? InvolvedUserId { get; set; }

    #nullable disable
    
    /// <summary>
    /// A description, stating the reason of this moderation action.
    /// </summary>
    public string Description { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}