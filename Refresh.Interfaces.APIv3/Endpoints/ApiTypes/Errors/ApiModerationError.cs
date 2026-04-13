namespace Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;

public class ApiModerationError : ApiError
{
    public ApiModerationError(string message) : base(message, UnprocessableContent) {}

    public const string AssetAutoFlaggedErrorWhen = "This content was flagged as potentially unsafe, and administrators have been alerted. If you believe this is an error, please contact an administrator.";
    public static readonly ApiModerationError AssetAutoFlaggedError = new(AssetAutoFlaggedErrorWhen);

    public const string AssetDisallowedErrorWhen = "The asset you tried to upload is disallowed.";
    public static readonly ApiModerationError AssetDisallowedError = new(AssetDisallowedErrorWhen);
}