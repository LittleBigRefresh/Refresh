namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiAdminRenameUserRequest : ApiAdminRequest
{
    public string? Username { get; set; }
    public bool? DisallowOldUsername { get; set; }
}