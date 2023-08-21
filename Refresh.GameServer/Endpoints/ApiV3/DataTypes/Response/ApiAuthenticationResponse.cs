#nullable disable
using JetBrains.Annotations;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
public class ApiAuthenticationResponse : IApiAuthenticationResponse
{
    public string TokenData { get; set; }
    [CanBeNull] public string RefreshTokenData { get; set; }
    public string UserId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}