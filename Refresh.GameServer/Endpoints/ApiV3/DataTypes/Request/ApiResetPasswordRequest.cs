#nullable disable
namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;

[Serializable]
public class ApiResetPasswordRequest
{
    public string PasswordSha512 { get; set; }
    public string ResetToken { get; set; }
}