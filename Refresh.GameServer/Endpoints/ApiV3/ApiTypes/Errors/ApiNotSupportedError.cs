namespace Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;

public class ApiNotSupportedError : ApiError
{
    public static readonly ApiNotSupportedError Instance = new();

    public const string OAuthProviderTokenRevocationUnsupportedErrorWhen = "This OAuth provider does not support token revocation";
    public static readonly ApiNotSupportedError OAuthProviderTokenRevocationUnsupportedError = new(OAuthProviderTokenRevocationUnsupportedErrorWhen);

    public const string OAuthProviderDisabledErrorWhen = "The server does not have this OAuth provider enabled";
    public static readonly ApiNotSupportedError OAuthProviderDisabledError = new(OAuthProviderDisabledErrorWhen);

    private ApiNotSupportedError() : base("The server is not configured to support this endpoint.", NotImplemented)
    {}
    
    private ApiNotSupportedError(string message) : base(message, NotImplemented)
    {}
}