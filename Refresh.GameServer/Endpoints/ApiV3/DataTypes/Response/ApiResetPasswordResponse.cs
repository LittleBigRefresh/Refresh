#nullable disable
namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[Serializable]
public class ApiResetPasswordResponse : IApiAuthenticationResponse
{
    public string Reason { get; set; }
    public string ResetToken { get; set; }
}