using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Serialization;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses.Serialization;
using Bunkum.Listener.Protocol;
using Refresh.Common.Time;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Core.Services;
using Refresh.Core.Types.Matching;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Contests;
using Refresh.Database.Models.Notifications;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.Notifications;

namespace Refresh.Interfaces.Game.Endpoints;

public class AnnouncementEndpoints : EndpointGroup
{
    private static bool AnnounceGetNotifications(StringBuilder output, GameDatabaseContext database, GameUser user, GameServerConfig config)
    {
        List<GameNotification> notifications = database.GetNotificationsByUser(user, 5, 0).Items.ToList();
        int count = database.GetNotificationCountByUser(user);
        if (count == 0) return false;

        string s = count != 1 ? "s" : string.Empty;

        output.Append($"Howdy, {user.Username}. You have {count} notification{s}:\n\n");
        for (int i = 0; i < notifications.Count; i++)
        {
            GameNotification notification = notifications[i];
            output.Append($"  {notification.Title} ({i + 1}/{count}):\n");
            output.Append($"    {notification.Text}\n\n");
        }

        output.Append($"To view more, or clear these notifications, you can visit the website at {config.WebExternalUrl}!\n");
        return true;
    }

    private static bool AnnounceGetAnnouncements(StringBuilder output, GameDatabaseContext database)
    {
        IEnumerable<GameAnnouncement> announcements = database.GetAnnouncements().ToList();
        foreach (GameAnnouncement announcement in announcements)
            output.Append($"{announcement.Title}: {announcement.Text}\n");
        
        return announcements.Any();
    }
    
    private static bool AnnounceGetContest(StringBuilder output, Token token, GameDatabaseContext database, GameServerConfig config)
    {
        GameContest? contest = database.GetNewestActiveContest();
        if (contest == null) return false;
        
        // only show contests for the current game
        if (!contest.AllowedGames.Contains(token.TokenGame)) return false;
        
        output.Append("There's a contest live right now!\n\n");
        output.AppendLine($"** {contest.ContestTitle} **");
        
        output.Append("Summary: ");
        output.AppendLine(contest.ContestSummary);
        if (!string.IsNullOrWhiteSpace(contest.ContestTheme))
        {
            output.Append("Theme: ");
            output.AppendLine(contest.ContestTheme);
        }
        
        output.AppendLine($"See more on the website: {config.WebExternalUrl}/contests/{contest.ContestId}");
        
        return true;
    }

    [GameEndpoint("announce")]
    [MinimumRole(GameUserRole.Restricted)]
    [SuppressMessage("ReSharper", "RedundantAssignment")]
    public string Announce(RequestContext context, GameServerConfig config, GameUser user, GameDatabaseContext database, Token token, IDateTimeProvider timeProvider)
    {
        if (user.Role == GameUserRole.Restricted)
        {
            return $"""
                   Your account is currently in restricted mode.
                   
                   Reason: {user.BanReason ?? "No reason given."}
                   Remaining: ~{(user.BanExpiryDate! - timeProvider.Now).Value.Days} days
                   
                   You can still play, but you won't be able to publish levels, post comments, or otherwise interact with the community.
                   For more information, please contact an administrator.
                   """;
        }

        if (!user.EmailAddressVerified)
        {
            return $"Your account doesn't have a verified email address. If this is a new account, there should be a verification code in your inbox.\n\n" +
                   
                   $"You can still play online without a verified email address if you wish, " +
                   $"but you might miss out on 'share' features like level uploading, and you will not be able to dive in.\n\n" +
                   
                   $"If you didn't receive it, try checking your spam folder. You can also opt to resend the code on the website.\n\n" +
                   
                   $"For more information, sign into the site at {config.WebExternalUrl}.";
        }
        
        // ReSharper disable once JoinDeclarationAndInitializer (makes it easier to follow)
        bool appended;
        StringBuilder output = new();
        
        appended = AnnounceGetAnnouncements(output, database);
        
        if (appended) output.Append('\n');
        appended = AnnounceGetContest(output, token, database, config);
        
        // All games except PSP support real-time notifications.
        // If we're playing on PSP, check for notifications.
        if (token.TokenGame == TokenGame.LittleBigPlanetPSP)
        {
            if (appended) output.Append('\n');
            appended = AnnounceGetNotifications(output, database, user, config);
        }
        
        return output.ToString();
    }

    [GameEndpoint("notification", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public string Notification(RequestContext context, GameServerConfig config, Token token, GameDatabaseContext database, MatchService matchService)
    {
        // On LBP1 the only regular ticking request is /notification,
        // so we update the "last contact" of the user's room when we receive a notification request to prevent LBP1 rooms from being auto-closed early
        GameRoom? room = matchService.RoomAccessor.GetRoomByUser(token.User, token.TokenPlatform, token.TokenGame);
        
        if (room != null)
        {
            room.LastContact = DateTimeOffset.Now;
            
            matchService.RoomAccessor.UpdateRoom(room);
        }
        
        DatabaseList<GameNotification> notifications = database.GetNotificationsByUser(token.User, 3, 0);
        
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