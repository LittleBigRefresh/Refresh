#nullable disable
namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request.Authentication;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiResetPasswordRequest
{
    public string PasswordSha512 { get; set; }
    public string ResetToken { get; set; }
}