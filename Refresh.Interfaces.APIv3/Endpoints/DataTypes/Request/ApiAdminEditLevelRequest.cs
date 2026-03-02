using Refresh.Database.Models.Authentication;
using Refresh.Database.Query;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiAdminEditLevelRequest : IApiAdminEditLevelRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? IconHash { get; set; }
    public TokenGame? GameVersion { get; set; }
    public bool? IsReUpload { get; set; }
    public string? OriginalPublisher { get; set; }
}