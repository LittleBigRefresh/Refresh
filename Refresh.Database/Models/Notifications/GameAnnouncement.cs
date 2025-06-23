using MongoDB.Bson;

namespace Refresh.Database.Models.Notifications;

#nullable disable

/// <summary>
/// An announcement posted by a server administrator.
/// </summary>
public partial class GameAnnouncement
{
    [Key] public ObjectId AnnouncementId { get; set; } = ObjectId.GenerateNewId();
    public string Title { get; set; }
    public string Text { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}