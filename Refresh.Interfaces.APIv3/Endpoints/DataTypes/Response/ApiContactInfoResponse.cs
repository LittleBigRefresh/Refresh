namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiContactInfoResponse : IApiResponse
{
    /// <summary>
    /// The owner's screen name.
    /// </summary>
    public required string AdminName { get; set; }
    /// <summary>
    /// The owner's email address.
    /// </summary>
    public required string EmailAddress { get; set; }
    /// <summary>
    /// A link to a Discord server.
    /// </summary>
    public required string? DiscordServerInvite { get; set; }
    /// <summary>
    /// The owner's personal Discord account.
    /// </summary>
    public required string? AdminDiscordUsername { get; set; }
}