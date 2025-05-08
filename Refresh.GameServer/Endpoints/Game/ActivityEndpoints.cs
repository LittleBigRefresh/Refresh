using System.Xml;
using System.Xml.Serialization;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.News;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class ActivityEndpoints : EndpointGroup
{
    [GameEndpoint("stream", ContentType.Xml)]
    [GameEndpoint("stream", ContentType.Xml, HttpMethods.Post)]
    [NullStatusCode(BadRequest)]
    [MinimumRole(GameUserRole.Restricted)]
    public ActivityPage? GetRecentActivity(RequestContext context, GameServerConfig config, GameDatabaseContext database, GameUser? user,
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

        return ActivityPage.GameUserActivity(database, new ActivityQueryParameters
        {
            Timestamp = timestamp,
            EndTimestamp = endTimestamp,
            ExcludeFriends = excludeFriends,
            ExcludeMyLevels = excludeMyLevels,
            ExcludeFavouriteUsers = excludeFavouriteUsers,
            ExcludeMyself = excludeMyself,
            User = user,
        }, dataContext);
    }

    [GameEndpoint("stream/slot/{type}/{id}", ContentType.Xml)]
    [NullStatusCode(BadRequest)]
    [MinimumRole(GameUserRole.Restricted)]
    public Response GetRecentActivityForLevel(RequestContext context, GameServerConfig config, GameDatabaseContext database, GameUser? user,
        string type, int id, DataContext dataContext)
    {
        if (!config.PermitShowingOnlineUsers)
            return Unauthorized;
        
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

        ActivityPage page = ActivityPage.GameForLevelActivity(database, level, new ActivityQueryParameters
        {
            Count = 20,
            Skip = 0,
            Timestamp = timestamp,
            EndTimestamp = endTimestamp,
            ExcludeFriends = excludeFriends,
            ExcludeFavouriteUsers = excludeFavouriteUsers,
            ExcludeMyself = excludeMyself,
            User = user,
        }, dataContext);
        
        return new Response(page, ContentType.Xml);
    }

    [GameEndpoint("stream/user2/{username}", ContentType.Xml)]
    [NullStatusCode(BadRequest)]
    [MinimumRole(GameUserRole.Restricted)]
    public Response GetRecentActivityFromUser(RequestContext context, GameServerConfig config, GameDatabaseContext database, string username,
        DataContext dataContext)
    {
        if (!config.PermitShowingOnlineUsers)
            return Unauthorized;

        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return NotFound;

        if (user.FakeUser)
            return new Response(new ActivityPage());
        
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

        return new Response(ActivityPage.GameFromUserActivity(database, new ActivityQueryParameters
        {
            Timestamp = timestamp,
            EndTimestamp = endTimestamp,
            ExcludeFriends = excludeFriends,
            ExcludeMyLevels = excludeMyLevels,
            ExcludeFavouriteUsers = excludeFavouriteUsers,
            ExcludeMyself = excludeMyself,
            User = user,
        }, dataContext), ContentType.Xml);
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
            IEnumerable<GameLevel> teamPicks = database.GetTeamPickedLevels(32, 0, null, new LevelFilterSettings(token.TokenGame)).Items;
            
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