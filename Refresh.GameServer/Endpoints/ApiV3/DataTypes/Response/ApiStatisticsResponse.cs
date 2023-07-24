namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiStatisticsResponse : IApiResponse
{
    public int TotalLevels { get; set; }
    public int TotalUsers { get; set; }
    public int CurrentRoomCount { get; set; }
    public int CurrentIngamePlayersCount { get; set; }
}