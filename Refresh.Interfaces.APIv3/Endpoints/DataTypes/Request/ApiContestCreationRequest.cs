using Refresh.Database.Models.Authentication;
using Refresh.Database.Query;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;

#nullable disable

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiContestCreationRequest : IContestCreationInfo
{
    public string OrganizerId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public string ContestTag { get; set; }
    public string ContestTitle { get; set; }
    public string ContestSummary { get; set; }

    #nullable restore

    public string? BannerUrl { get; set; }
    public string? ContestDetails { get; set; }
    public string? ContestTheme { get; set; }
    public TokenGame[]? AllowedGames { get; set; }
    public int? TemplateLevelId { get; set; }
}