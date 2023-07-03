namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Errors;

public class ApiInternalError : ApiError
{
    public ApiInternalError(string message) : base(message, InternalServerError)
    {}
}