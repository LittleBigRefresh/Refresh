namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request.Authentication;

#nullable disable

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiSendPasswordResetEmailRequest
{
    public string EmailAddress { get; set; }
}