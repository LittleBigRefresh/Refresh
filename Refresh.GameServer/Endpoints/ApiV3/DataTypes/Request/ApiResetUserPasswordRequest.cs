namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;

#nullable disable

public class ApiResetUserPasswordRequest
{
    public string PasswordSha512 { get; set; }
}