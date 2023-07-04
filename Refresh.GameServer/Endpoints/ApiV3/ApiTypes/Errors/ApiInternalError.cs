namespace Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;

public class ApiInternalError : ApiError
{
    public ApiInternalError(string message) : base(message, InternalServerError)
    {}
}