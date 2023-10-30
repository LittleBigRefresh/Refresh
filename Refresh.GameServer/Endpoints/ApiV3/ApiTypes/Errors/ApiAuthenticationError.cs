namespace Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiAuthenticationError : ApiError
{
    public const string NoPermissionsForObjectWhen = "You do not lack the permissions to manage or view this resource.";
    public static readonly ApiAuthenticationError NoPermissionsForObject = new(NoPermissionsForObjectWhen);
    
    public bool Warning { get; init; }

    public ApiAuthenticationError(string message, bool warning = false) : base(message, Forbidden)
    {
        this.Warning = warning;
    }
}