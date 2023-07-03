namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Errors;

public class ApiValidationError : ApiError
{
    public ApiValidationError(string message) : base(message) {}
}