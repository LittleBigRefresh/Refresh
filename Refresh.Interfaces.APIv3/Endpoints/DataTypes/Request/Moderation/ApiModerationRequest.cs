namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request.Moderation;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiModerationRequest
{
    public string? Reason { get; set; }
}