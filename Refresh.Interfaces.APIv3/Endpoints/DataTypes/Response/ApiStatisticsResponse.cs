namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiStatisticsResponse : IApiResponse
{
    public required int TotalLevels { get; set; }
    public required int ModdedLevels { get; set; }
    public required int TotalUsers { get; set; }
    public required int ActiveUsers { get; set; }
    public required int TotalPhotos { get; set; }
    public required int TotalEvents { get; set; }
    public required int CurrentRoomCount { get; set; }
    public required int CurrentIngamePlayersCount { get; set; }
    
    public required ApiRequestStatisticsResponse RequestStatistics { get; set; }
}