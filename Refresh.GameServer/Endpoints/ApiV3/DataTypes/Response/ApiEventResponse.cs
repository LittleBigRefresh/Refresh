using Refresh.GameServer.Types.Activity;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiEventResponse : IApiResponse, IDataConvertableFrom<ApiEventResponse, Event>
{
    public required string EventId { get; set; }
    public required EventType EventType { get; set; }
    public required DateTimeOffset OccuredAt { get; set; }
    public required EventDataType StoredDataType { get; set; }
    public required int? StoredSequentialId { get; set; }
    public required string? StoredObjectId { get; set; }
    
    public static ApiEventResponse? FromOld(Event? old)
    {
        if (old == null) return null;

        return new ApiEventResponse
        {
            EventId = old.EventId.ToString()!,
            EventType = old.EventType,
            OccuredAt = DateTimeOffset.FromUnixTimeMilliseconds(old.Timestamp),
            StoredDataType = old.StoredDataType,
            StoredSequentialId = old.StoredSequentialId,
            StoredObjectId = old.StoredObjectId?.ToString(),
        };
    }

    public static IEnumerable<ApiEventResponse> FromOldList(IEnumerable<Event> oldList) => oldList.Select(FromOld)!;
}