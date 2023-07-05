#nullable disable
namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiAuthenticationRequest
{
    public string Username { get; set; }
    public string PasswordSha512 { get; set; }
}