namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Admin;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiAdminUserPlanetsResponse : IApiResponse
{
    public string Lbp2PlanetsHash { get; set; } = "0";
    public string Lbp3PlanetsHash { get; set; } = "0";
    public string VitaPlanetsHash { get; set; } = "0";
    public string BetaPlanetsHash { get; set; } = "0";

    public bool AreLbp2PlanetsModded { get; set; }
    public bool AreLbp3PlanetsModded { get; set; }
    public bool AreVitaPlanetsModded { get; set; }
    public bool AreBetaPlanetsModded { get; set; }
}