#nullable disable
namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request.Authentication;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiAuthenticationRequest
{
    public string EmailAddress { get; set; }
    public string PasswordSha512 { get; set; }
}