namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.OAuth;

public class ApiOAuthBeginAuthenticationResponse : IApiResponse
{
    public required string AuthorizationUrl { get; set; }
}