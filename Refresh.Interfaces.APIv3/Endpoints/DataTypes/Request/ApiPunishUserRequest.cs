namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;

#nullable disable

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiPunishUserRequest
{
    public DateTimeOffset ExpiryDate { get; set; }
    public string Reason { get; set; }
}