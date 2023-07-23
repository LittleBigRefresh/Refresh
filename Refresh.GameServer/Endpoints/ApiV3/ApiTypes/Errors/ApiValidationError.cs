namespace Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;

public class ApiValidationError : ApiError
{
    public const string ObjectIdParseErrorWhen = "The object's ID could not be parsed by the server";
    public static readonly ApiValidationError ObjectIdParseError = new(ObjectIdParseErrorWhen);
    
    public const string BooleanParseErrorWhen = "The boolean could not be parsed by the server";
    public static readonly ApiValidationError BooleanParseError = new(BooleanParseErrorWhen);
    
    public const string NumberParseErrorWhen = "The number could not be parsed by the server";
    public static readonly ApiValidationError NumberParseError = new(NumberParseErrorWhen);
    
    public const string UserMissingErrorWhen = "The user could not be found";
    public static readonly ApiValidationError UserMissingError = new(UserMissingErrorWhen);

    public const string LevelMissingErrorWhen = "The level could not be found";
    public static readonly ApiValidationError LevelMissingError = new(LevelMissingErrorWhen);
    
    public const string ScoreMissingErrorWhen = "The score could not be found";
    public static readonly ApiValidationError ScoreMissingError = new(ScoreMissingErrorWhen);
    
    public const string PhotoMissingErrorWhen = "The photo could not be found";
    public static readonly ApiValidationError PhotoMissingError = new(PhotoMissingErrorWhen);

    public const string NoPhotoDeletionPermissionErrorWhen = "You do not have permission to delete someone else's photo";
    public static readonly ApiValidationError NoPhotoDeletionPermissionError = new(NoPhotoDeletionPermissionErrorWhen);
    
    public ApiValidationError(string message) : base(message) {}
}