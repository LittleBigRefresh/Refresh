namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public sealed class ApiSetLevelAuthorRequest
{
    public string AuthorId { get; set; } = string.Empty;
}