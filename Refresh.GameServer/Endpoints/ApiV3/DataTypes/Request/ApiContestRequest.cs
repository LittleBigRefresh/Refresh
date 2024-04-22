namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiContestRequest
{
    public string? OrganizerId { get; set; }
    public DateTimeOffset? CreationDate { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string? ContestTag { get; set; }
    public string? BannerUrl { get; set; }
    public string? ContestTitle { get; set; }
    public string? ContestSummary { get; set; }
    public string? ContestDetails { get; set; }
}