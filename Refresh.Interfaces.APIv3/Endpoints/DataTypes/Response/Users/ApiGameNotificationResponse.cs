using Refresh.Core.Types.Data;
using Refresh.Database.Models.Notifications;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameNotificationResponse : IApiResponse, IDataConvertableFrom<ApiGameNotificationResponse, GameNotification>
{
    public required string NotificationId { get; set; }
    public required string Title { get; set; }
    public required string Text { get; set; }
    
    public required DateTimeOffset CreatedAt { get; set; }
    
    public required string FontAwesomeIcon { get; set; }
    
    public static ApiGameNotificationResponse? FromOld(GameNotification? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiGameNotificationResponse
        {
            NotificationId = old.NotificationId.ToString()!,
            Title = old.Title,
            Text = old.Text,
            CreatedAt = old.CreatedAt,
            FontAwesomeIcon = old.FontAwesomeIcon,
        };
    }

    public static IEnumerable<ApiGameNotificationResponse> FromOldList(IEnumerable<GameNotification> oldList,
        DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}