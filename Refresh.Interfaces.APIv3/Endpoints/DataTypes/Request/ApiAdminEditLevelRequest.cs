using Refresh.Database.Models.Authentication;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiAdminEditLevelRequest : ApiEditLevelRequest
{
    public TokenGame? GameVersion { get; set; }
    public bool? IsReUpload { get; set; }
    public string? OriginalPublisher { get; set; }
}