using Refresh.GameServer.Authentication;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiContestRequest
{
    public string? OrganizerId { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string? ContestTag { get; set; }
    public string? BannerUrl { get; set; }
    public string? ContestTitle { get; set; }
    public string? ContestSummary { get; set; }
    public string? ContestDetails { get; set; }
    public string? ContestTheme { get; set; }
    public TokenGame[]? AllowedGames { get; set; }
    public int? TemplateLevelId { get; set; }
}