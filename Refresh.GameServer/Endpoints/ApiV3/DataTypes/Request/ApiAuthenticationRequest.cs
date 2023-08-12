#nullable disable
namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiAuthenticationRequest
{
    public string EmailAddress { get; set; }
    public string PasswordSha512 { get; set; }
}