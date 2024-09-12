namespace Refresh.GameServer.Types.OAuth2;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class OAuth2ErrorResponse
{
    public string Error { get; set; }
    public string? ErrorDescription { get; set; }
    public string? ErrorUri { get; set; }
}