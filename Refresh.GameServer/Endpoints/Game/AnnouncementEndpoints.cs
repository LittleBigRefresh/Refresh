using System.Xml.Serialization;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Endpoints.Debugging;
using Bunkum.Core.Responses.Serialization;
using Bunkum.Listener.Protocol;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Notifications;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class AnnouncementEndpoints : EndpointGroup
{
    private static string AnnounceGetNotifications(IGameDatabaseContext database, GameUser user, GameServerConfig config)
    {
        List<GameNotification> notifications = database.GetNotificationsByUser(user, 5, 0).Items.ToList();
        int count = database.GetNotificationCountByUser(user);
        if (count == 0) return string.Empty;

        string s = count != 1 ? "s" : string.Empty;

        string notificationText = $"Howdy, {user.Username}. You have {count} notification{s}:\n\n";
        for (int i = 0; i < notifications.Count; i++)
        {
            GameNotification notification = notifications[i];
            notificationText += $"  {notification.Title} ({i + 1}/{count}):\n" +
                                $"    {notification.Text}\n\n";
        }

        notificationText += $"To view more, or clear these notifications, you can visit the website at {config.WebExternalUrl}!\n";

        return notificationText;
    }

    private static string AnnounceGetAnnouncements(IGameDatabaseContext database)
    {
        IEnumerable<GameAnnouncement> announcements = database.GetAnnouncements();
        // it's time to allocate
        return announcements.Aggregate(string.Empty, (current, announcement) => current + $"{announcement.Title}: {announcement.Text}\n");
    }

    [GameEndpoint("announce")]
    [MinimumRole(GameUserRole.Restricted)]
    public string Announce(RequestContext context, GameServerConfig config, GameUser user, IGameDatabaseContext database, Token token)
    {
        if (user.Role == GameUserRole.Restricted)
        {
            return "Your account is currently in restricted mode.\n\n" +
                   "You can still play, but you won't be able to publish levels, post comments," +
                   "or otherwise interact with the community." +
                   "For more information, please contact an administrator.";
        }
        
        string announcements = AnnounceGetAnnouncements(database);
        
        // All games except PSP support real-time notifications.
        // If we're not playing on PSP, move forward to check for notifications.
        if (token.TokenGame != TokenGame.LittleBigPlanetPSP) return announcements;
        
        string notifications = AnnounceGetNotifications(database, user, config);

        if (announcements.Length == 0) return notifications;
        if (notifications.Length == 0) return announcements;
        return announcements + "\n" + notifications; // I HATE IT WHYYYYYYYYYYYY
    }

    [GameEndpoint("notification", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public string Notification(RequestContext context, GameServerConfig config, GameUser user, IGameDatabaseContext database)
    {
        DatabaseList<GameNotification> notifications = database.GetNotificationsByUser(user, 3, 0);
        // ReSharper disable once LoopCanBeConvertedToQuery (makes it unreadable)
        
        using MemoryStream ms = new();
        using BunkumXmlTextWriter bunkumXmlTextWriter = new(ms);

        XmlSerializer serializer = new(typeof(SerializedNotification));
        
        XmlSerializerNamespaces namespaces = new();
        namespaces.Add("", "");
        
        foreach (GameNotification notification in notifications.Items)
        {
            SerializedNotification serializedNotification = new()
            {
                Text = $"[{config.InstanceName}] {notification.Title}: {notification.Text}",
            };
                
            serializer.Serialize(bunkumXmlTextWriter, serializedNotification, namespaces);
            database.DeleteNotification(notification);
        }

        ms.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(ms);
        
        return reader.ReadToEnd();
    }
}