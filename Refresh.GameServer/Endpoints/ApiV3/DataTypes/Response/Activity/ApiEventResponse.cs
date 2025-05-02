using Refresh.Database.Models.Activity;
using Refresh.GameServer.Types.Data;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Activity;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiEventResponse : IApiResponse, IDataConvertableFrom<ApiEventResponse, Event>
{
    public required string EventId { get; set; }
    public required int EventType { get; set; }
    public required string UserId { get; set; }
    public required DateTimeOffset OccurredAt { get; set; }
    public required EventDataType StoredDataType { get; set; }
    public required int? StoredSequentialId { get; set; }
    public required string? StoredObjectId { get; set; }
    
    public static ApiEventResponse? FromOld(Event? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiEventResponse
        {
            EventId = old.EventId.ToString()!,
            EventType = (int)old.EventType,
            UserId = old.User.UserId.ToString()!,
            OccurredAt = old.Timestamp,
            StoredDataType = old.StoredDataType,
            StoredSequentialId = old.StoredSequentialId,
            StoredObjectId = old.StoredObjectId?.ToString(),
        };
    }

    public static IEnumerable<ApiEventResponse> FromOldList(IEnumerable<Event> oldList, DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}