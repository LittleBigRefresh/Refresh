namespace Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;

public class ApiModerationError : ApiError
{
    public static readonly ApiModerationError Instance = new();
    
    public ApiModerationError() : base("This content was flagged as potentially unsafe, and administrators have been alerted. If you believe this is an error, please contact an administrator.", UnprocessableContent)
    {
    }
}