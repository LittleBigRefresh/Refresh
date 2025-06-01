namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Admin;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiAdminUserPlanetsResponse : IApiResponse
{
    public string Lbp2PlanetsHash { get; set; } = "0";
    public string Lbp3PlanetsHash { get; set; } = "0";
    public string VitaPlanetsHash { get; set; } = "0";
}