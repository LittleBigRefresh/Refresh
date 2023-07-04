#nullable disable
namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[Serializable]
public class ApiAuthenticationResponse : IApiAuthenticationResponse
{
    public string TokenData { get; set; }
    public string UserId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}