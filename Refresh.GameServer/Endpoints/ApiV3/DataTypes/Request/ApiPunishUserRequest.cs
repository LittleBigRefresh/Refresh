namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;

#nullable disable

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiPunishUserRequest
{
    public string UserId { get; set; }
    public DateTimeOffset ExpiryDate { get; set; }
    public string Reason { get; set; }
}