namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.OAuth2.Discord;

public class ApiDiscordOAuth2BeginAuthenticationResponse : IApiResponse
{
    public required string AuthorizationUrl { get; set; }
}