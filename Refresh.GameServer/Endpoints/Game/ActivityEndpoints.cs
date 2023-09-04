using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Time;
using Realms.Sync;
using Refresh.GameServer.Database;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.News;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class ActivityEndpoints : EndpointGroup
{
    [GameEndpoint("stream", ContentType.Xml)]
    [NullStatusCode(BadRequest)]
    [MinimumRole(GameUserRole.Restricted)]
    public ActivityPage? GetRecentActivity(RequestContext context, GameDatabaseContext database)
    {
        long timestamp = 0;
        long endTimestamp = 0;

        string? tsStr = context.QueryString["timestamp"];
        string? tsEndStr = context.QueryString["endTimestamp"];
        if (tsStr != null && !long.TryParse(tsStr, out timestamp)) return null;
        if (tsEndStr != null && !long.TryParse(tsEndStr, out endTimestamp)) return null;

        if (endTimestamp == 0) endTimestamp = timestamp - 86400000 * 7; // 1 week

        ActivityPage page = new(database, timestamp: timestamp, endTimestamp: endTimestamp);
        return page;
    }

    [GameEndpoint("news", ContentType.Xml)]
    [Authentication(false)]
    [MinimumRole(GameUserRole.Restricted)]
    public GameNewsResponse GetNews(RequestContext context, IDateTimeProvider time)
    {
        return new GameNewsResponse
        {
            Subcategory = new GameNewsSubcategory
            {
                Title = "Subcategory title",
                Id = 1,
                Item = new GameNewsItem
                {
                    Id = 2,
                    Subject = "Item subject",
                    Content = new GameNewsItemContent
                    {
                        Frame = new GameNewsFrame
                        {
                            Title = "Frame item title",
                            Width = 1024,
                            Item = new GameNewsFrameItem
                            {
                                Background = "Frame item background",
                                Content = "Frame item content",
                                Handle = new SerializedUserHandle
                                {
                                    Username = "Username",
                                    IconHash = "0",
                                },
                                Level = new GameNewsFrameItemSlot
                                {
                                    Id = 0,
                                    Type = "user",
                                },
                                Width = 1024,
                            },
                        },
                    },
                },
            },
        };
    }
}