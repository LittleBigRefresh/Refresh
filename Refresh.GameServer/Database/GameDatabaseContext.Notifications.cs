using JetBrains.Annotations;
using MongoDB.Bson;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Notifications;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Notifications
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
            CreatedAt = this._time.Now,
        };

        this._realm.Write(() =>
        {
            this._realm.Add(notification);
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
        this._realm.All<GameNotification>()
            .Count(n => n.User == user);
    
    [Pure]
    public DatabaseList<GameNotification> GetNotificationsByUser(GameUser user, int count, int skip) =>
        new(this._realm.All<GameNotification>().Where(n => n.User == user), skip, count);

    [Pure]
    public GameNotification? GetNotificationByUuid(GameUser user, ObjectId id) 
        => this._realm
            .All<GameNotification>()
            .FirstOrDefault(n => n.User == user && n.NotificationId == id);
    
    public void DeleteNotificationsByUser(GameUser user)
    {
        this._realm.Write(() =>
        {
            this._realm.RemoveRange(this._realm.All<GameNotification>().Where(n => n.User == user));
        });
    }
    
    public void DeleteNotification(GameNotification notification)
    {
        this._realm.Write(() =>
        {
            this._realm.Remove(notification);
        });
    }

    public IEnumerable<GameAnnouncement> GetAnnouncements() => this._realm.All<GameAnnouncement>();
    
    public GameAnnouncement? GetAnnouncementById(ObjectId id) => this._realm.All<GameAnnouncement>().FirstOrDefault(a => a.AnnouncementId == id);
    
    public GameAnnouncement AddAnnouncement(string title, string text)
    {
        GameAnnouncement announcement = new()
        {
            AnnouncementId = ObjectId.GenerateNewId(),
            Title = title,
            Text = text,
            CreatedAt = this._time.Now,
        };
        
        this._realm.Write(() =>
        {
            this._realm.Add(announcement);
        });

        return announcement;
    }
    
    public void DeleteAnnouncement(GameAnnouncement announcement)
    {
        this._realm.Write(() =>
        {
            this._realm.Remove(announcement);
        });
    }
}