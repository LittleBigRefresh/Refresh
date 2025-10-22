using System.Xml;
using System.Xml.Serialization;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.RateLimit;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Common.Time;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;
using Refresh.Interfaces.Game.Types.Activity;
using Refresh.Interfaces.Game.Types.Activity.Groups;
using Refresh.Interfaces.Game.Types.Lists;
using Refresh.Interfaces.Game.Types.News;

namespace Refresh.Interfaces.Game.Endpoints;

public class ActivityEndpoints : EndpointGroup
{
    private const int RequestTimeoutDuration = 60;
    private const int MaxRequestAmount = 80;
    private const int RequestBlockDuration = 60;
    private const string BucketName = "activity";

    [GameEndpoint("stream", ContentType.Xml)]
    [GameEndpoint("stream", ContentType.Xml, HttpMethods.Post)]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, BucketName)]
    [NullStatusCode(BadRequest)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedActivityPage? GetRecentActivity(RequestContext context, GameServerConfig config, GameDatabaseContext database, GameUser? user,
        DataContext dataContext)
    {
        if (!config.PermitShowingOnlineUsers)
            return null;
        
        long timestamp = 0;
        long endTimestamp = 0;

        bool excludeMyLevels = bool.Parse(context.QueryString["excludeMyLevels"] ?? "false");
        bool excludeFriends = bool.Parse(context.QueryString["excludeFriends"] ?? "false");
        bool excludeFavouriteUsers = bool.Parse(context.QueryString["excludeFavouriteUsers"] ?? "false");
        bool excludeMyself = bool.Parse(context.QueryString["excludeMyself"] ?? "false");

        string? tsStr = context.QueryString["timestamp"];
        string? tsEndStr = context.QueryString["endTimestamp"];
        if (tsStr != null && !long.TryParse(tsStr, out timestamp)) return null;
        if (tsEndStr != null && !long.TryParse(tsEndStr, out endTimestamp)) return null;

        if (endTimestamp == 0) endTimestamp = timestamp - 86400000 * 7; // 1 week

        return SerializedActivityPage.FromOld(database.GetUserRecentActivity(new ActivityQueryParameters
        {
            Timestamp = timestamp,
            EndTimestamp = endTimestamp,
            ExcludeFriends = excludeFriends,
            ExcludeMyLevels = excludeMyLevels,
            ExcludeFavouriteUsers = excludeFavouriteUsers,
            ExcludeMyself = excludeMyself,
            User = user,
            QuerySource = ActivityQuerySource.Game,
        }), dataContext);
    }

    [GameEndpoint("stream/slot/{type}/{id}", ContentType.Xml)]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, BucketName)]
    [NullStatusCode(BadRequest)]
    [MinimumRole(GameUserRole.Restricted)]
    public Response GetRecentActivityForLevel(RequestContext context, GameServerConfig config, GameDatabaseContext database, GameUser? user,
        string type, int id, DataContext dataContext)
    {
        if (!config.PermitShowingOnlineUsers)
            return Unauthorized;

        return NoContent;
        
        // FIXME: enable below code to have this endpoint return actual recent activity as soon as we find out how to do so without
        // the game rejecting the response and spamming this endpoint
        /*
        GameLevel? level = type == "developer" ? database.GetStoryLevelById(id) : database.GetLevelById(id);
        if (level == null) return NotFound;
        
        long timestamp = 0;
        long endTimestamp = 0;

        bool excludeFriends = bool.Parse(context.QueryString["excludeFriends"] ?? "false");
        bool excludeFavouriteUsers = bool.Parse(context.QueryString["excludeFavouriteUsers"] ?? "false");
        bool excludeMyself = bool.Parse(context.QueryString["excludeMyself"] ?? "false");
        
        string? tsStr = context.QueryString["timestamp"];
        string? tsEndStr = context.QueryString["endTimestamp"];
        if (tsStr != null && !long.TryParse(tsStr, out timestamp)) return BadRequest;
        if (tsEndStr != null && !long.TryParse(tsEndStr, out endTimestamp)) return BadRequest;

        if (endTimestamp == 0) endTimestamp = timestamp - 86400000 * 7; // 1 week

        SerializedActivityPage? page = SerializedActivityPage.FromOld(database.GetRecentActivityForLevel(level, new ActivityQueryParameters
        {
            Count = 20,
            Skip = 0,
            Timestamp = timestamp,
            EndTimestamp = endTimestamp,
            ExcludeFriends = excludeFriends,
            ExcludeFavouriteUsers = excludeFavouriteUsers,
            ExcludeMyself = excludeMyself,
            User = user,
            QuerySource = ActivityQuerySource.Game,
        }), dataContext);
        
        return new Response(page, ContentType.Xml);
        */
    }

    [GameEndpoint("stream/user2/{username}", ContentType.Xml)]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, BucketName)]
    [NullStatusCode(BadRequest)]
    [MinimumRole(GameUserRole.Restricted)]
    public Response GetRecentActivityFromUser(RequestContext context, GameServerConfig config, GameDatabaseContext database, string username,
        DataContext dataContext)
    {
        if (!config.PermitShowingOnlineUsers)
            return Unauthorized;
        
        return NoContent;

        // FIXME: enable below code to have this endpoint return actual recent activity as soon as we find out how to do so without
        // the game rejecting the response and spamming this endpoint
        /*
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return NotFound;

        if (user.FakeUser)
            return new Response(new SerializedActivityPage
            {
                StartTimestamp = 0,
                EndTimestamp = 0,
                Groups = new SerializedActivityGroups(),
                Users = new SerializedUserList(),
                Levels = new SerializedLevelList(),
            });
        
        long timestamp = 0;
        long endTimestamp = 0;

        bool excludeMyLevels = bool.Parse(context.QueryString["excludeMyLevels"] ?? "false");
        bool excludeFriends = bool.Parse(context.QueryString["excludeFriends"] ?? "false");
        bool excludeFavouriteUsers = bool.Parse(context.QueryString["excludeFavouriteUsers"] ?? "false");
        bool excludeMyself = bool.Parse(context.QueryString["excludeMyself"] ?? "false");
        
        string? tsStr = context.QueryString["timestamp"];
        string? tsEndStr = context.QueryString["endTimestamp"];
        if (tsStr != null && !long.TryParse(tsStr, out timestamp)) return BadRequest;
        if (tsEndStr != null && !long.TryParse(tsEndStr, out endTimestamp)) return BadRequest;

        if (endTimestamp == 0) endTimestamp = timestamp - 86400000 * 7; // 1 week

        return new Response(SerializedActivityPage.FromOld(database.GetRecentActivityFromUser(new ActivityQueryParameters
        {
            Timestamp = timestamp,
            EndTimestamp = endTimestamp,
            ExcludeFriends = excludeFriends,
            ExcludeMyLevels = excludeMyLevels,
            ExcludeFavouriteUsers = excludeFavouriteUsers,
            ExcludeMyself = excludeMyself,
            User = user,
            QuerySource = ActivityQuerySource.Game,
        }), dataContext), ContentType.Xml);
        */
    }
    
    [GameEndpoint("news", ContentType.Xml)]
    [Authentication(false)]
    [MinimumRole(GameUserRole.Restricted)]
    public Response GetNews(RequestContext context, GameDatabaseContext database, IDateTimeProvider time, Token? token)
    {
        List<GameNewsItem> items = new();

        XmlSerializer itemContentSerializer = new(typeof(GameNewsItemContent));

        if (token != null)
        {
            IEnumerable<GameLevel> teamPicks = database.GetTeamPickedLevels(32, 0, null, new(token.TokenGame)).Items;
            
            int i = 0;
            foreach (GameLevel level in teamPicks)
            {
                XmlDocument doc = new();

                using (XmlWriter writer = doc.CreateNavigator()!.AppendChild())
                {
                    itemContentSerializer.Serialize(writer, new GameNewsItemContent
                    {
                        Frame = new GameNewsFrame
                        {
                            Width = 100,
                            Title = "Team pick",
                            Item = new GameNewsFrameItem
                            {
                                Width = 100,
                                Content = $"{level.Title} has been team picked!",
                                Background = "Frame item background",
                                Level = new GameNewsFrameItemSlot
                                {
                                    Id = level.LevelId,
                                    Type = "user",
                                },
                            },
                        },
                    });
                }

                items.Add(new GameNewsItem
                {
                    Id = i,
                    Subject = "Team Pick",
                    Content = doc.DocumentElement!.InnerXml,
                    Timestamp = time.TimestampSeconds,
                });

                i++;
            }
        }

        return new Response(new GameNewsResponse
        {
            Subcategory = new GameNewsSubcategory
            {
                Title = "Subcategory title",
                Id = 1,
                Items = items,
            },
        }, ContentType.Xml);
    }
}