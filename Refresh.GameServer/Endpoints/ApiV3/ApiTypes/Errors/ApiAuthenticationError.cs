namespace Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiAuthenticationError : ApiError
{
    public bool Warning { get; init; }

    public ApiAuthenticationError(string message, bool warning = false) : base(message, Forbidden)
    {
        this.Warning = warning;
    }
}