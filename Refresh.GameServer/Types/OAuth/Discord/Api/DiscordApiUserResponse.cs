namespace Refresh.GameServer.Types.OAuth.Discord.Api;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class DiscordApiUserResponse
{
    /// <summary>
    /// The user's ID, as a snowflake
    /// </summary>
    public ulong Id { get; set; }
    /// <summary>
    /// The user's username
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// The user's discord tag
    /// </summary>
    public string Discriminator { get; set; }
    /// <summary>
    /// The user's global name, if set
    /// </summary>
    public string? GlobalName { get; set; }
    /// <summary>
    /// The hash of the user's avatar
    /// </summary>
    public string? Avatar { get; set; }
    /// <summary>
    /// The hash of the user's banner
    /// </summary>
    public string? Banner { get; set; }
    /// <summary>
    /// The user's accent colour
    /// </summary>
    public uint? AccentColor { get; set; }
}