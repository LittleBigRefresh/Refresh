namespace Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;

public class ApiValidationError : ApiError
{
    public const string ObjectIdParseErrorWhen = "The object's ID could not be parsed by the server";
    public static readonly ApiValidationError ObjectIdParseError = new(ObjectIdParseErrorWhen);
    
    public const string BooleanParseErrorWhen = "The boolean could not be parsed by the server";
    public static readonly ApiValidationError BooleanParseError = new(BooleanParseErrorWhen);
    
    public const string NumberParseErrorWhen = "The number could not be parsed by the server";
    public static readonly ApiValidationError NumberParseError = new(NumberParseErrorWhen);
    
    public const string IpAddressParseErrorWhen = "The IP address could not be parsed by the server";
    public static readonly ApiValidationError IpAddressParseError = new(IpAddressParseErrorWhen);

    public const string NoPhotoDeletionPermissionErrorWhen = "You do not have permission to delete someone else's photo";
    public static readonly ApiValidationError NoPhotoDeletionPermissionError = new(NoPhotoDeletionPermissionErrorWhen);
    
    public const string HashInvalidErrorWhen = "The hash is invalid (should be SHA1 hash)";
    public static readonly ApiValidationError HashInvalidError = new(HashInvalidErrorWhen);
    
    public const string HashMissingErrorWhen = "The hash is missing or null";
    public static readonly ApiValidationError HashMissingError = new(HashMissingErrorWhen);

    public const string BodyTooLongErrorWhen = "The asset must be under 2MB";
    public static readonly ApiValidationError BodyTooLongError = new(BodyTooLongErrorWhen);

    public const string CannotReadAssetErrorWhen = "The asset could not be read";
    public static readonly ApiValidationError CannotReadAssetError = new(CannotReadAssetErrorWhen);

    public const string BodyMustBeImageErrorWhen = "The asset must be a PNG/JPEG file";
    public static readonly ApiValidationError BodyMustBeImageError = new(BodyMustBeImageErrorWhen);
    
    public const string ResourceExistsErrorWhen = "The resource you are attempting to create already exists.";
    public static readonly ApiValidationError ResourceExistsError = new(ResourceExistsErrorWhen);
    
    public const string InvalidTextureGuidErrorWhen = "The passed GUID is not a valid texture GUID for the specified game.";
    public static readonly ApiValidationError InvalidTextureGuidError = new(InvalidTextureGuidErrorWhen);
    
    public ApiValidationError(string message) : base(message) {}
}