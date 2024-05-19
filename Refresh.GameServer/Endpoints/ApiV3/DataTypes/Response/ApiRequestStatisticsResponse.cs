using Refresh.GameServer.Types;
using Refresh.GameServer.Types.Data;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiRequestStatisticsResponse : IApiResponse, IDataConvertableFrom<ApiRequestStatisticsResponse, RequestStatistics>
{
    public required long TotalRequests { get; set; }
    public required long ApiRequests { get; set; }
    public required long GameRequests { get; set; }
    
    public static ApiRequestStatisticsResponse? FromOld(RequestStatistics? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiRequestStatisticsResponse
        {
            TotalRequests = old.TotalRequests,
            ApiRequests = old.ApiRequests,
            GameRequests = old.GameRequests,
        };
    }

    public static IEnumerable<ApiRequestStatisticsResponse> FromOldList(IEnumerable<RequestStatistics> oldList,
        DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}