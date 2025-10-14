namespace Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;

public class ApiValidationError : ApiError
{
    public const string ObjectIdParseErrorWhen = "The object's ID could not be parsed by the server";
    public static readonly ApiValidationError ObjectIdParseError = new(ObjectIdParseErrorWhen);
    
    public const string BooleanParseErrorWhen = "The boolean could not be parsed by the server";
    public static readonly ApiValidationError BooleanParseError = new(BooleanParseErrorWhen);
    
    public const string NumberParseErrorWhen = "The number could not be parsed by the server";
    public static readonly ApiValidationError NumberParseError = new(NumberParseErrorWhen);

    public const string RatingParseErrorWhen = "The given rating was invalid";
    public static readonly ApiValidationError RatingParseError = new(RatingParseErrorWhen);

    public const string IpAddressParseErrorWhen = "The IP address could not be parsed by the server";
    public static readonly ApiValidationError IpAddressParseError = new(IpAddressParseErrorWhen);

    public const string NoPhotoDeletionPermissionErrorWhen = "You do not have permission to delete someone else's photo";
    public static readonly ApiValidationError NoPhotoDeletionPermissionError = new(NoPhotoDeletionPermissionErrorWhen);

    public const string NoCommentDeletionPermissionErrorWhen = "You do not have permission to delete this comment";
    public static readonly ApiValidationError NoCommentDeletionPermissionError = new(NoCommentDeletionPermissionErrorWhen);

    public const string NoReviewEditPermissionErrorWhen = "You do not have permission to edit this review";
    public static readonly ApiValidationError NoReviewEditPermissionError = new(NoReviewEditPermissionErrorWhen);

    public const string NoReviewDeletionPermissionErrorWhen = "You do not have permission to delete this review";
    public static readonly ApiValidationError NoReviewDeletionPermissionError = new(NoReviewDeletionPermissionErrorWhen);

    public const string DontRateOwnContentWhen = "You may not rate your own content";
    public static readonly ApiValidationError DontRateOwnContent = new(DontRateOwnContentWhen);

    public const string DontReviewOwnLevelWhen = "You may not review your own level";
    public static readonly ApiValidationError DontReviewOwnLevel = new(DontReviewOwnLevelWhen);

    public const string DontReviewLevelBeforePlayingWhen = "You may not review levels you haven't played yet";
    public static readonly ApiValidationError DontReviewLevelBeforePlaying = new(DontReviewLevelBeforePlayingWhen);

    public const string ReviewHasInvalidLabelsWhen = "Your review contained invalid labels";
    public static readonly ApiValidationError ReviewHasInvalidLabels = new(ReviewHasInvalidLabelsWhen);
    
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

    public const string EmailDoesNotActuallyExistErrorWhen = "The email address given does not exist. Are you sure you typed it in correctly?";
    public static readonly ApiValidationError EmailDoesNotActuallyExistError = new(EmailDoesNotActuallyExistErrorWhen);

    public const string BadUserLookupIdTypeWhen = "The ID type used to specify the user is not supported";
    public static readonly ApiValidationError BadUserLookupIdType = new(BadUserLookupIdTypeWhen);
    
    public ApiValidationError(string message) : base(message) {}
}