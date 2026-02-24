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

    public const string InvalidPlaylistIdWhen = "The playlist ID couldn't be parsed by the server";
    public static readonly ApiValidationError InvalidPlaylistId = new(InvalidPlaylistIdWhen);

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

    public const string IconMustBeImageErrorWhen = "The icon must be a PNG/JPEG file";
    public static readonly ApiValidationError IconMustBeImageError = new(IconMustBeImageErrorWhen);

    public const string IconMissingErrorWhen = "The icon is missing from the server";
    public static readonly ApiValidationError IconMissingError = new(IconMissingErrorWhen);
    
    public const string ResourceExistsErrorWhen = "The resource you are attempting to create already exists.";
    public static readonly ApiValidationError ResourceExistsError = new(ResourceExistsErrorWhen);
    
    public const string InvalidTextureGuidErrorWhen = "The passed GUID is not a valid texture GUID for the specified game.";
    public static readonly ApiValidationError InvalidTextureGuidError = new(InvalidTextureGuidErrorWhen);

    public const string EmailDoesNotActuallyExistErrorWhen = "The email address given does not exist. Are you sure you typed it in correctly?";
    public static readonly ApiValidationError EmailDoesNotActuallyExistError = new(EmailDoesNotActuallyExistErrorWhen);

    public const string MayNotOverwriteRoleErrorWhen = "You may not overwrite user roles because you are not an admin";
    public static readonly ApiValidationError MayNotOverwriteRoleError = new(MayNotOverwriteRoleErrorWhen);

    public const string MayNotModifyUserDueToLowRoleErrorWhen = "You may not modify this user because their role is above yours, or the same as yours incase you're not an admin.";
    public static readonly ApiValidationError MayNotModifyUserDueToLowRoleError = new(MayNotModifyUserDueToLowRoleErrorWhen);

    public const string WrongRoleUpdateMethodErrorWhen = "The specified role cannot be assigned to the user using this endpoint.";
    public static readonly ApiValidationError WrongRoleUpdateMethodError = new(WrongRoleUpdateMethodErrorWhen);

    public const string UserIsAlreadyPardonedErrorWhen = "This user has no punishments, they are already pardoned.";
    public static readonly ApiValidationError UserIsAlreadyPardonedError = new(UserIsAlreadyPardonedErrorWhen);

    public const string RoleMissingErrorWhen = "The specified role does not exist.";
    public static readonly ApiValidationError RoleMissingError = new(RoleMissingErrorWhen);

    public const string ContestOrganizerIdParseErrorWhen = "The organizer's user ID could not be parsed by the server";
    public static readonly ApiValidationError ContestOrganizerIdParseError = new(ContestOrganizerIdParseErrorWhen);

    public const string ContestDataMissingErrorWhen = "The contest must at least have a title, aswell as a start and end date specified";
    public static readonly ApiValidationError ContestDataMissingError = new(ContestDataMissingErrorWhen);

    public const string InvalidUsernameErrorWhen = "The username must be valid. The requirements are 3 to 16 alphanumeric characters, plus hyphens and underscores.";
    public static readonly ApiValidationError InvalidUsernameError = new(InvalidUsernameErrorWhen);

    public const string UsernameTakenErrorWhen = "This username is already taken!";
    public static readonly ApiValidationError UsernameTakenError = new(UsernameTakenErrorWhen);

    public const string NoPlaylistEditPermissionErrorWhen = "You do not have permission to update this playlist";
    public static readonly ApiValidationError NoPlaylistEditPermissionError = new(NoPlaylistEditPermissionErrorWhen);

    public const string NoPlaylistDeletePermissionErrorWhen = "You do not have permission to delete this playlist";
    public static readonly ApiValidationError NoPlaylistDeletionPermissionError = new(NoPlaylistDeletePermissionErrorWhen);

    // TODO: Split off error messages which are actually 401 or anything else that isn't 400
    public ApiValidationError(string message) : base(message) {}
}
