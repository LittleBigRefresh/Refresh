namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

/// <summary>
/// An error indicating that a resource was not able to be found.
/// </summary>
public class ApiNotFoundError : ApiError
{
    public ApiNotFoundError() : base("This requested resource was not found.", NotFound)
    {}
}