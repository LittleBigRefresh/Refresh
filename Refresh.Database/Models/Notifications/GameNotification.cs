using MongoDB.Bson;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Notifications;

#nullable disable

[JsonObject(MemberSerialization.OptOut)]
[Serializable]
public partial class GameNotification : IRealmObject
{
    [Key] public ObjectId NotificationId { get; set; } = ObjectId.GenerateNewId();
    public string Title { get; set; }
    public string Text { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    [JsonIgnore] public GameUser User { get; set; }
    
    public string FontAwesomeIcon { get; set; }
}