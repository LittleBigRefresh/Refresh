namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiListInformation
{
    public int EntryCount { get; set; }
    public int NextPageIndex { get; set; }
}