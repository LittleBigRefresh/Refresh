namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request.Authentication;

#nullable disable

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiRegisterRequest
{
    public string Username { get; set; }
    public string EmailAddress { get; set; }
    public string PasswordSha512 { get; set; }
}