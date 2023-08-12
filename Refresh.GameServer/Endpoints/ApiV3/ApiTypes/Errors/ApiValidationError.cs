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
    
    public ApiValidationError(string message) : base(message) {}
}