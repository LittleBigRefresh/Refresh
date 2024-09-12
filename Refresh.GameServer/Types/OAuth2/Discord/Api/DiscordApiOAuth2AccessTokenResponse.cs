namespace Refresh.GameServer.Types.OAuth2.Discord.Api;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class DiscordApiOAuth2AccessTokenResponse
{
    public string AccessToken { get; set; }
    public string TokenType { get; set; }
    public int ExpiresIn { get; set; }
    public string RefreshToken { get; set; }
    public string Scope { get; set; }
}