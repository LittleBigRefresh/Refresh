using System.Net;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

/// <summary>
/// An error indicating a problem with the server, your query, or anything else.
/// </summary>
[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiError
{
    public ApiError(string message, HttpStatusCode code = BadRequest)
    {
        this.Message = message;
        this.StatusCode = code;
    }

    /// <summary>
    /// A description of the error.
    /// </summary>
    public string Message { get; set; }
    
    /// <summary>
    /// A numerical status code of the error.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }
}