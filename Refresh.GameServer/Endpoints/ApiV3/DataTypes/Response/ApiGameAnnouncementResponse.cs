using Refresh.GameServer.Types.Notifications;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameAnnouncementResponse : IApiResponse
{
    public required string AnnouncementId { get; set; }
    public required string Title { get; set; }
    public required string Text { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    
    public static ApiGameAnnouncementResponse? FromOld(GameAnnouncement? old)
    {
        if (old == null) return null;

        return new ApiGameAnnouncementResponse
        {
            AnnouncementId = old.AnnouncementId.ToString()!,
            Title = old.Title,
            Text = old.Text,
            CreatedAt = old.CreatedAt,
        };
    }

    public static IEnumerable<ApiGameAnnouncementResponse> FromOldList(IEnumerable<GameAnnouncement> oldList) => oldList.Select(FromOld).ToList()!;
}