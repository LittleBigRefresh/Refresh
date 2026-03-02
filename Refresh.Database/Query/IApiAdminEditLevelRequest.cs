using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Query;

public interface IApiAdminEditLevelRequest
{
    string? Title { get; set; }
    string? Description { get; set; }
    string? IconHash { get; set; }
    public TokenGame? GameVersion { get; set; }
    public bool? IsReUpload { get; set; }
    public string? OriginalPublisher { get; set; }
}