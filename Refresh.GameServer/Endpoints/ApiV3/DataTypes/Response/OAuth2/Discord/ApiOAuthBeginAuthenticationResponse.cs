namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.OAuth2.Discord;

public class ApiOAuthBeginAuthenticationResponse : IApiResponse
{
    public required string AuthorizationUrl { get; set; }
}