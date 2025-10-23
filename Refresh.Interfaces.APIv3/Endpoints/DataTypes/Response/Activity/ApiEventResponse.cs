using Refresh.Core.Types.Data;
using Refresh.Database.Models.Activity;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Activity;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiEventResponse : IApiResponse, IDataConvertableFrom<ApiEventResponse, Event>
{
    public required string EventId { get; set; }
    public required int EventType { get; set; }
    public required EventOverType OverType { get; set; }
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
            OverType = old.OverType,
            UserId = old.User.UserId.ToString()!,
            OccurredAt = old.Timestamp,
            StoredDataType = old.StoredDataType,
            StoredSequentialId = old.StoredSequentialId,
            StoredObjectId = old.StoredObjectId?.ToString(),
        };
    }

    public static IEnumerable<ApiEventResponse> FromOldList(IEnumerable<Event> oldList, DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}