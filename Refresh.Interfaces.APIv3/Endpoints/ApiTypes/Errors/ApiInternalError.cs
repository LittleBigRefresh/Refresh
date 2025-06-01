namespace Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;

public class ApiInternalError : ApiError
{
    public const string CouldNotGetAssetDatabaseErrorWhen = "An error occurred while retrieving the asset from the database";
    public static readonly ApiInternalError CouldNotGetAssetDatabaseError = new(CouldNotGetAssetDatabaseErrorWhen);
    
    public const string CouldNotGetAssetErrorWhen = "An error occurred while retrieving the asset from the data store";
    public static readonly ApiInternalError CouldNotGetAssetError = new(CouldNotGetAssetErrorWhen);

    public const string CouldNotWriteAssetErrorWhen = "An error occurred while saving the asset to the data store";
    public static readonly ApiInternalError CouldNotWriteAssetError = new(CouldNotWriteAssetErrorWhen);

    public const string HashNotFoundInDatabaseErrorWhen = "The hash was present on the server, but could not be found in the database.";
    public static readonly ApiInternalError HashNotFoundInDatabaseError = new(HashNotFoundInDatabaseErrorWhen);
    
    public ApiInternalError(string message) : base(message, InternalServerError)
    {}
}