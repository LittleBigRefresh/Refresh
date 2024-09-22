using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.OAuth.Discord.Api;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.OAuth.Discord;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiDiscordUserResponse : IApiResponse, IDataConvertableFrom<ApiDiscordUserResponse, DiscordApiUserResponse>
{
    /// <summary>
    /// The user's ID, as a snowflake
    /// </summary>
    public required ulong Id { get; set; }
    /// <summary>
    /// The user's username
    /// </summary>
    public required string Username { get; set; }
    /// <summary>
    /// The user's discord tag
    /// </summary>
    public required string Discriminator { get; set; }
    /// <summary>
    /// The user's global name, if set
    /// </summary>
    public required string? GlobalName { get; set; }
    /// <summary>
    /// The hash of the user's avatar
    /// </summary>
    public required string? AvatarUrl { get; set; }
    /// <summary>
    /// The hash of the user's banner
    /// </summary>
    public required string? Banner { get; set; }
    /// <summary>
    /// The user's accent colour
    /// </summary>
    public required uint? AccentColor { get; set; }
    
    public static ApiDiscordUserResponse? FromOld(DiscordApiUserResponse? old, DataContext dataContext)
    {
        if (old == null)
            return null;

        return new ApiDiscordUserResponse
        {
            Id = old.Id,
            Username = old.Username,
            Discriminator = old.Discriminator,
            GlobalName = old.GlobalName,
            AvatarUrl = $"https://cdn.discordapp.com/avatars/{old.Id}/{old.Avatar}?size=512",
            Banner = old.Banner,
            AccentColor = old.AccentColor,
        };
    }

    public static IEnumerable<ApiDiscordUserResponse> FromOldList(IEnumerable<DiscordApiUserResponse> oldList,
        DataContext dataContext) => oldList.Select(d => FromOld(d, dataContext)!);
}