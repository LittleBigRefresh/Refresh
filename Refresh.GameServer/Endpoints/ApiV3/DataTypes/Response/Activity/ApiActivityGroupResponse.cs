using Refresh.Database.Models.Activity;
using Refresh.GameServer.Types.Data;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Activity;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiActivityGroupResponse : IApiResponse, IDataConvertableFrom<ApiActivityGroupResponse, DatabaseActivityGroup>
{
    public required string Type { get; set; }
    public required DateTimeOffset Timestamp { get; set; }
    public required IEnumerable<ApiEventResponse> Events { get; set; }
    public required IEnumerable<ApiActivityGroupResponse> Children { get; set; }
    
    public static ApiActivityGroupResponse? FromOld(DatabaseActivityGroup? old, DataContext dataContext)
    {
        if (old == null)
            return null;

        return new ApiActivityGroupResponse
        {
            Type = old.GroupType,
            Timestamp = old.Timestamp,
            Events = ApiEventResponse.FromOldList(old.Events, dataContext),
            Children = FromOldList(old.Children, dataContext),
        };
    }

    public static IEnumerable<ApiActivityGroupResponse> FromOldList(IEnumerable<DatabaseActivityGroup> oldList, DataContext dataContext)
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}