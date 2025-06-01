using Refresh.Core.Types.Data;
using Refresh.Database.Models;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response;

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