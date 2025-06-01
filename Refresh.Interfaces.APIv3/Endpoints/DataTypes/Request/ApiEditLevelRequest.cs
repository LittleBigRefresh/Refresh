using Refresh.Database.Query;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiEditLevelRequest : IApiEditLevelRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? IconHash { get; set; }
}