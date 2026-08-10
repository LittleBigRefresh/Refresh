namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request.Moderation;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiDisallowAssetRequest : ApiModerationRequest
{
    public string? Type { get; set; }
}