#nullable disable
namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request.Authentication;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiResetPasswordRequest
{
    public string PasswordSha512 { get; set; }
    public string ResetToken { get; set; }
}