using JetBrains.Annotations;

namespace Refresh.GameServer.Types.OAuth;

#nullable disable

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class OAuth2ErrorResponse
{
    public string Error { get; set; }
    [CanBeNull] public string ErrorDescription { get; set; }
    [CanBeNull] public string ErrorUri { get; set; }
}
