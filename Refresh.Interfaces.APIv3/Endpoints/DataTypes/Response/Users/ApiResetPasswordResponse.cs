#nullable disable
namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiResetPasswordResponse : IApiAuthenticationResponse
{
    public string Reason { get; set; }
    public string ResetToken { get; set; }
}