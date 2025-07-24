using MongoDB.Bson;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Notifications;

#nullable disable

[JsonObject(MemberSerialization.OptOut)]
[Serializable]
public partial class GameNotification
{
    [Key] public ObjectId NotificationId { get; set; } = ObjectId.GenerateNewId();
    public string Title { get; set; }
    public string Text { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    [ForeignKey(nameof(UserId)), Required]
    public GameUser User { get; set; }
    [Required]
    public ObjectId UserId { get; set; }
    
    public string FontAwesomeIcon { get; set; }
}