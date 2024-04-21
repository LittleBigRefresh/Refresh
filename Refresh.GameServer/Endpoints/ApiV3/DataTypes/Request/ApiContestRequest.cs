namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiContestRequest
{
    public required string OrganizerId { get; set; }
    public required DateTimeOffset CreationDate { get; set; }
    public required DateTimeOffset StartDate { get; set; }
    public required DateTimeOffset EndDate { get; set; }
    public required string ContestTag { get; set; }
    public required string BannerUrl { get; set; }
    public required string ContestTitle { get; set; }
    public required string ContestSummary { get; set; }
    public required string ContestDetails { get; set; }
}