using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Notifications;

#nullable disable

public partial class GameNotification : IRealmObject
{
    public ObjectId NotificationId { get; set; } = ObjectId.GenerateNewId();
    public string Title { get; set; }
    public string Text { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public GameUser User { get; set; }
    
    public string FontAwesomeIcon { get; set; }
    public string ColorCode { get; set; }
}