using Refresh.Database.Query;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiEditLevelRequest : IApiEditLevelRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? IconHash { get; set; }
}