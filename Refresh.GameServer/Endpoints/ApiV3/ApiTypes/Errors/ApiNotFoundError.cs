namespace Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;

/// <summary>
/// An error indicating that a resource was not able to be found.
/// </summary>
public class ApiNotFoundError : ApiError
{
    public static readonly ApiNotFoundError Instance = new();
    
    public const string UserMissingErrorWhen = "The user could not be found";
    public static readonly ApiNotFoundError UserMissingError = new(UserMissingErrorWhen);

    public const string LevelMissingErrorWhen = "The level could not be found";
    public static readonly ApiNotFoundError LevelMissingError = new(LevelMissingErrorWhen);
    
    public const string ScoreMissingErrorWhen = "The score could not be found";
    public static readonly ApiNotFoundError ScoreMissingError = new(ScoreMissingErrorWhen);
    
    public const string PhotoMissingErrorWhen = "The photo could not be found";
    public static readonly ApiNotFoundError PhotoMissingError = new(PhotoMissingErrorWhen);
    
    public const string ContestMissingErrorWhen = "The contest could not be found";
    public static readonly ApiNotFoundError ContestMissingError = new(ContestMissingErrorWhen);

    public const string DiscordOAuth2TokenMissingErrorWhen = "A discord OAuth2 token for this user could not be found";
    public static readonly ApiNotFoundError DiscordOAuth2TokenMissingError = new(DiscordOAuth2TokenMissingErrorWhen);

    private ApiNotFoundError() : base("The requested resource was not found", NotFound)
    {}
    
    private ApiNotFoundError(string message) : base(message, NotFound)
    {}
}