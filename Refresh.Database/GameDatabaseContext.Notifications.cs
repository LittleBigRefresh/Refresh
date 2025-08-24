using JetBrains.Annotations;
using MongoDB.Bson;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Notifications;

namespace Refresh.Database;

public partial class GameDatabaseContext // Notifications
{
    public void AddNotification(string title, string text, GameUser user, string? icon = null)
    {
        icon ??= "bell";

        GameNotification notification = new()
        {
            Title = title,
            Text = text,
            UserId = user.UserId,
            FontAwesomeIcon = icon,
            CreatedAt = this._time.Now,
        };
        
        this.GameNotifications.Add(notification);
        this.SaveChanges();
    }

    public void AddErrorNotification(string title, string text, GameUser user)
    {
        this.AddNotification(title, text, user, "exclamation-circle");
    }

    public void AddPublishFailNotification(string reason, string levelTitle, GameUser user)
    {
        string title = string.IsNullOrWhiteSpace(levelTitle) ? "Unnamed Level" : levelTitle;
        this.AddErrorNotification("Publish failed", $"The level '{title}' failed to publish. {reason}", user);
    }

    private const string LoginFail = "Log-in failure";
    
    public void AddLoginFailNotification(string reason, GameUser user)
    {
        this.AddErrorNotification(LoginFail, $"There was a recent failed sign-in attempt. {reason}", user);
    }

    public void ClearLoginFailNotificationsForUser(GameUser user)
    {
        this.Write(() =>
        {
            this.GameNotifications.RemoveRange(n => n.User == user && n.Title == LoginFail);
        });
    }

    [Pure]
    public int GetNotificationCountByUser(GameUser user) => 
        this.GameNotifications
            .Count(n => n.User == user);
    
    [Pure]
    public DatabaseList<GameNotification> GetNotificationsByUser(GameUser user, int count, int skip) =>
        new(this.GameNotifications.Where(n => n.User == user), skip, count);

    [Pure]
    public GameNotification? GetNotificationByUuid(GameUser user, ObjectId id) 
        => this.GameNotifications
            .FirstOrDefault(n => n.User == user && n.NotificationId == id);
    
    public void DeleteNotificationsByUser(GameUser user)
    {
        this.Write(() =>
        {
            this.GameNotifications.RemoveRange(n => n.User == user);
        });
    }
    
    public void DeleteNotification(GameNotification notification)
    {
        this.Write(() =>
        {
            this.GameNotifications.Remove(notification);
        });
    }

    public IEnumerable<GameAnnouncement> GetAnnouncements() => this.GameAnnouncements;
    
    public GameAnnouncement? GetAnnouncementById(ObjectId id) => this.GameAnnouncements.FirstOrDefault(a => a.AnnouncementId == id);
    
    public GameAnnouncement AddAnnouncement(string title, string text)
    {
        GameAnnouncement announcement = new()
        {
            AnnouncementId = ObjectId.GenerateNewId(),
            Title = title,
            Text = text,
            CreatedAt = this._time.Now,
        };
        
        this.Write(() =>
        {
            this.GameAnnouncements.Add(announcement);
        });

        return announcement;
    }
    
    public void DeleteAnnouncement(GameAnnouncement announcement)
    {
        this.Write(() =>
        {
            this.GameAnnouncements.Remove(announcement);
        });
    }
}