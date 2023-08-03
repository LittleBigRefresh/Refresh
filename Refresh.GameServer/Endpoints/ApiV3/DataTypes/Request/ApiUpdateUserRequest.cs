namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiUpdateUserRequest
{
    public string? Description { get; set; }
    public bool? AllowIpAuthentication { get; set; }
}