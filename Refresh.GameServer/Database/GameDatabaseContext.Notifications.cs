using JetBrains.Annotations;
using MongoDB.Bson;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Notifications;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial interface IGameDatabaseContext // Notifications
{
    public void AddNotification(string title, string text, GameUser user, string? icon = null)
    {
        icon ??= "bell";

        GameNotification notification = new()
        {
            Title = title,
            Text = text,
            User = user,
            FontAwesomeIcon = icon,
            CreatedAt = this.Time.Now,
        };

        this.Write(() =>
        {
            this.Add(notification);
        });
    }

    public void AddErrorNotification(string title, string text, GameUser user)
    {
        this.AddNotification(title, text, user, "exclamation-circle");
    }

    public void AddPublishFailNotification(string reason, GameLevel body, GameUser user)
    {
        this.AddErrorNotification("Publish failed", $"The level '{body.Title}' failed to publish. {reason}", user);
    }
    
    public void AddLoginFailNotification(string reason, GameUser user)
    {
        this.AddErrorNotification("Authentication failure", $"There was a recent failed sign-in attempt. {reason}", user);
    }

    [Pure]
    public int GetNotificationCountByUser(GameUser user) => 
        this.All<GameNotification>()
            .Count(n => n.User == user);
    
    [Pure]
    public DatabaseList<GameNotification> GetNotificationsByUser(GameUser user, int count, int skip) =>
        new(this.All<GameNotification>().Where(n => n.User == user), skip, count);

    [Pure]
    public GameNotification? GetNotificationByUuid(GameUser user, ObjectId id) 
        => this.All<GameNotification>()
            .FirstOrDefault(n => n.User == user && n.NotificationId == id);
    
    public void DeleteNotificationsByUser(GameUser user)
    {
        this.Write(() =>
        {
            this.RemoveRange(this.All<GameNotification>().Where(n => n.User == user));
        });
    }
    
    public void DeleteNotification(GameNotification notification)
    {
        this.Write(() =>
        {
            this.Remove(notification);
        });
    }

    public IEnumerable<GameAnnouncement> GetAnnouncements() => this.All<GameAnnouncement>();
    
    public GameAnnouncement? GetAnnouncementById(ObjectId id) => this.All<GameAnnouncement>().FirstOrDefault(a => a.AnnouncementId == id);
    
    public GameAnnouncement AddAnnouncement(string title, string text)
    {
        GameAnnouncement announcement = new()
        {
            AnnouncementId = ObjectId.GenerateNewId(),
            Title = title,
            Text = text,
            CreatedAt = this.Time.Now,
        };
        
        this.Write(() =>
        {
            this.Add(announcement);
        });

        return announcement;
    }
    
    public void DeleteAnnouncement(GameAnnouncement announcement)
    {
        this.Write(() =>
        {
            this.Remove(announcement);
        });
    }
}