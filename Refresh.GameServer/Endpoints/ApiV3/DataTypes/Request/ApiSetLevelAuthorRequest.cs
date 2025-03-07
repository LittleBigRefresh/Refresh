namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public sealed class ApiSetLevelAuthorRequest
{
    public string AuthorName { get; set; } = string.Empty;
}