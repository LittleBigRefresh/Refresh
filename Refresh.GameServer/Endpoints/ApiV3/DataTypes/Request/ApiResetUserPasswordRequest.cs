namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;

public class ApiResetUserPasswordRequest
{
    public string PasswordSha512 { get; set; }
}