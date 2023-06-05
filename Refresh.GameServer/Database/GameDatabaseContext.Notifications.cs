using JetBrains.Annotations;
using MongoDB.Bson;
using Refresh.GameServer.Types.Notifications;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Notifications
{
    public void CreateNotification(string title, string text, GameUser user, string? icon = null, string? color = null)
    {
        icon ??= "bell";
        color ??= "#AA30F5";

        GameNotification notification = new()
        {
            Title = title,
            Text = text,
            User = user,
            ColorCode = color,
            FontAwesomeIcon = icon,
            CreatedAt = DateTimeOffset.Now,
        };

        this._realm.Write(() =>
        {
            this._realm.Add(notification);
        });
    }

    [Pure]
    public int GetNotificationCountByUser(GameUser user) => 
        this._realm.All<GameNotification>()
            .Count(n => n.User == user);
    
    [Pure]
    public IEnumerable<GameNotification> GetNotificationsByUser(GameUser user, int count, int skip) =>
        this._realm.All<GameNotification>()
            .Where(n => n.User == user)
            .AsEnumerable()
            .Skip(skip)
            .Take(count);

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
}