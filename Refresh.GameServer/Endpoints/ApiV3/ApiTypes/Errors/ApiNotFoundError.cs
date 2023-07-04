namespace Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;

/// <summary>
/// An error indicating that a resource was not able to be found.
/// </summary>
public class ApiNotFoundError : ApiError
{
    public static readonly ApiNotFoundError Instance = new();
    
    private ApiNotFoundError() : base("The requested resource was not found", NotFound)
    {}
}