using MongoDB.Bson;
using Realms;

namespace Refresh.GameServer.Types.Notifications;

#nullable disable

/// <summary>
/// An announcement posted by a server administrator.
/// </summary>
public partial class GameAnnouncement : IRealmObject
{
    [PrimaryKey] public ObjectId AnnouncementId { get; set; } = ObjectId.GenerateNewId();
    public string Title { get; set; }
    public string Text { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}