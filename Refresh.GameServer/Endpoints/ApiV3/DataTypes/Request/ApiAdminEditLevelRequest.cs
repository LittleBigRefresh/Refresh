using Refresh.GameServer.Authentication;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiAdminEditLevelRequest : ApiEditLevelRequest
{
    public TokenGame? GameVersion { get; set; }
}