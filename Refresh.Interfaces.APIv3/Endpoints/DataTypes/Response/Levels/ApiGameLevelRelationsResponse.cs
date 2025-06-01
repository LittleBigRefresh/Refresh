namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Levels;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameLevelRelationsResponse : IApiResponse
{
    public required bool IsHearted { get; set; }
    public required bool IsQueued { get; set; }
    public required int MyPlaysCount { get; set; }
}