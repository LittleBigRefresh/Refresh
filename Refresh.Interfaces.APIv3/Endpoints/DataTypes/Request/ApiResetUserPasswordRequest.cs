namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;

#nullable disable

public class ApiResetUserPasswordRequest
{
    public string PasswordSha512 { get; set; }
}