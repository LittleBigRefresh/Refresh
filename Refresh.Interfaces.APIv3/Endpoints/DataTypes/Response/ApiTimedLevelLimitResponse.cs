namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiTimedLevelLimitResponse : IApiResponse
{
    public required int TimeSpanHours { get; set; }
    public required int LevelQuota { get; set; }
}