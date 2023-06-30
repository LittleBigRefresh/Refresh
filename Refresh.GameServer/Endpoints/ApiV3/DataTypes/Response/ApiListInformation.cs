namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiListInformation
{
    public int NextPageIndex { get; set; }
    public int TotalItems { get; set; }
}