namespace Refresh.GameServer.Endpoints.ApiV3.ApiTypes;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiListInformation
{
    public int NextPageIndex { get; set; }
    public int TotalItems { get; set; }
}