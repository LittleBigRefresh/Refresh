namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Errors;

public class ApiValidationError : ApiError
{
    public const string ObjectIdParseErrorWhen = "The object's ID could not be parsed by the server";
    public static readonly ApiValidationError ObjectIdParseError = new(ObjectIdParseErrorWhen);
    
    public ApiValidationError(string message) : base(message) {}
}