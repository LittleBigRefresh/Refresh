namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public sealed class ApiSetLevelAuthorRequest
{
    public string AuthorId { get; set; } = string.Empty;
}