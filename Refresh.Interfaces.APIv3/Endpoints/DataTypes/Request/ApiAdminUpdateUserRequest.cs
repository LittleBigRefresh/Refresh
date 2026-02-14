using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiAdminUpdateUserRequest : IApiAdminEditUserRequest
{
    public string? Username { get; set; }
    public string? IconHash { get; set; }
    public string? VitaIconHash { get; set; }
    public string? BetaIconHash { get; set; }
    public string? Description { get; set; }
    public GameUserRole? Role { get; set; }
}