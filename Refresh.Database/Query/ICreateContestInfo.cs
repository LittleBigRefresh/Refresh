using Refresh.GameServer.Authentication;

namespace Refresh.Database.Query;

public interface ICreateContestInfo
{
    string? OrganizerId { get; set; }
    DateTimeOffset? StartDate { get; set; }
    DateTimeOffset? EndDate { get; set; }
    string? ContestTag { get; set; }
    string? BannerUrl { get; set; }
    string? ContestTitle { get; set; }
    string? ContestSummary { get; set; }
    string? ContestDetails { get; set; }
    string? ContestTheme { get; set; }
    TokenGame[]? AllowedGames { get; set; }
    int? TemplateLevelId { get; set; }
}