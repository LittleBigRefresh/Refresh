using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Notifications;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game.Handshake;

public class WelcomeEndpoints : EndpointGroup
{
    private const string AGPLLicense = """
    Copyright (C) 2023 LittleBigRefresh

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
    """;
    
    [GameEndpoint("eula")]
    [MinimumRole(GameUserRole.Restricted)]
    public string License(RequestContext context, GameServerConfig config)
    {
        return config.LicenseText + "\n\n" + AGPLLicense + "\n";
    }

    private static string AnnounceGetNotifications(GameDatabaseContext database, GameUser user, GameServerConfig config)
    {
        List<GameNotification> notifications = database.GetNotificationsByUser(user, 5, 0).Items.ToList();
        int count = database.GetNotificationCountByUser(user);
        if (count == 0) return "";

        string s = count != 1 ? "s" : "";

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

    private static string AnnounceGetAnnouncements(GameDatabaseContext database)
    {
        IEnumerable<GameAnnouncement> announcements = database.GetAnnouncements();
        // it's time to allocate
        return announcements.Aggregate("", (current, announcement) => current + $"{announcement.Title}: {announcement.Text}\n");
    }

    [GameEndpoint("announce")]
    [MinimumRole(GameUserRole.Restricted)]
    public string Announce(RequestContext context, GameServerConfig config, GameUser user, GameDatabaseContext database)
    {
        if (user.Role == GameUserRole.Restricted)
        {
            return "Your account is currently in restricted mode.\n\n" +
                   "You can still play, but you won't be able to publish levels, post comments," +
                   "or otherwise interact with the community." +
                   "For more information, please contact an administrator.";
        }
        
        string announcements = AnnounceGetAnnouncements(database);
        string notifications = AnnounceGetNotifications(database, user, config);

        if (announcements.Length == 0) return notifications;
        if (notifications.Length == 0) return announcements;
        return announcements + "\n" + notifications; // I HATE IT WHYYYYYYYYYYYY
    }
}