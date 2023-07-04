namespace Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;

public class ApiAuthenticationError : ApiError
{
    public ApiAuthenticationError(string message) : base(message, Unauthorized)
    {}
}