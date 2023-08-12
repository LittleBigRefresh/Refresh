namespace Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;

public class ApiAuthenticationError : ApiError
{
    public const string ApiNotAdminErrorWhen = "This endpoint requires administrative permissions, which you do not have.";
    public static readonly ApiAuthenticationError ApiNotAdminError = new(ApiNotAdminErrorWhen);
    
    public ApiAuthenticationError(string message) : base(message, Unauthorized)
    {}
}