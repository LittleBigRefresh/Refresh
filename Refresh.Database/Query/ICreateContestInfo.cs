using Refresh.Database.Models.Authentication;

namespace Refresh.Database.Query;

public interface ICreateContestInfo
{
    DateTimeOffset? StartDate { get; set; }
    DateTimeOffset? EndDate { get; set; }
    string? ContestTag { get; set; }
    string? BannerUrl { get; set; }
    string? ContestTitle { get; set; }
    string? ContestSummary { get; set; }
    string? ContestDetails { get; set; }
    string? ContestTheme { get; set; }
    TokenGame[]? AllowedGames { get; set; }
}