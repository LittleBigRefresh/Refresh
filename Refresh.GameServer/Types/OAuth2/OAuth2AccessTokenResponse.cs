using JetBrains.Annotations;

namespace Refresh.GameServer.Types.OAuth2;

#nullable disable

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class OAuth2AccessTokenResponse
{
    public string AccessToken { get; set; }
    public string TokenType { get; set; }
    public double ExpiresIn { get; set; }
    [CanBeNull] public string RefreshToken { get; set; }
    [CanBeNull] public string Scope { get; set; }
}