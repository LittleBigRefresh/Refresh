namespace Refresh.Interfaces.APIv3.Endpoints.ApiTypes;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiListInformation
{
    public int NextPageIndex { get; set; }
    public int TotalItems { get; set; }
}