namespace Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiAuthenticationError : ApiError
{
    public const string NoPermissionsForObjectWhen = "You lack the permissions to manage or view this resource.";
    public static readonly ApiAuthenticationError NoPermissionsForObject = new(NoPermissionsForObjectWhen);
    
    public const string NoPermissionsForCreationWhen = "You lack the permissions to create this type of resource.";
    public static readonly ApiAuthenticationError NoPermissionsForCreation = new(NoPermissionsForCreationWhen);
    
    public bool Warning { get; init; }

    public ApiAuthenticationError(string message, bool warning = false) : base(message, Forbidden)
    {
        this.Warning = warning;
    }
}