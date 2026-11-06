namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
/// <summary>
/// Contains (optional) request things shared by moderation endpoints which update or delete stuff.
/// </summary>
// TODO use this class itself (not a subclass) on already existing admin endpoints which take
// no JSON request body yet, once Bunkum stops throwing on empty JSON requests.
public class ApiAdminRequest
{
    public string? Reason { get; set; }
    public bool? Force { get; set; }
}