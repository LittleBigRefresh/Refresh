using System.Xml;
using System.Xml.Serialization;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Endpoints.Debugging;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Levels;
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
    public Response GetNews(RequestContext context, GameDatabaseContext database, IDateTimeProvider time, Token? token)
    {
        List<GameNewsItem> items = new();

        XmlSerializer itemContentSerializer = new(typeof(GameNewsItemContent));

        if (token != null)
        {
            IEnumerable<GameLevel> teamPicks = database.GetTeamPickedLevels(32, 0, token.TokenGame).Items;
            
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