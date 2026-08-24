namespace Refresh.Interfaces.APIv3.Documentation.Descriptions;

/// <summary>
/// Descriptions for commonly used API params.
/// </summary>
public static class SharedParamDescriptions
{
    public const string UserIdParam = "The UUID or username of the user.";
    public const string UserIdTypeParam = "The type of ID used to specify the user. Can be 'uuid', 'username' or 'name'.";
    public const string DomainToDisallowParam = "The email domain to disallow. If this is a whole address, only the part after the last @ will be used as the domain.";
}