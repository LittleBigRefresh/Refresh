using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.OAuth2.Discord.Api;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.OAuth2.Discord;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiDiscordOAuthUserResponse : IApiResponse, IDataConvertableFrom<ApiDiscordOAuthUserResponse, DiscordApiUserResponse>
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
    public required string? Avatar { get; set; }
    /// <summary>
    /// The hash of the user's banner
    /// </summary>
    public required string? Banner { get; set; }
    /// <summary>
    /// The user's accent colour
    /// </summary>
    public required uint? AccentColor { get; set; }
    
    public static ApiDiscordOAuthUserResponse? FromOld(DiscordApiUserResponse? old, DataContext dataContext)
    {
        if (old == null)
            return null;

        return new ApiDiscordOAuthUserResponse
        {
            Id = old.Id,
            Username = old.Username,
            Discriminator = old.Discriminator,
            GlobalName = old.GlobalName,
            Avatar = old.Avatar,
            Banner = old.Banner,
            AccentColor = old.AccentColor,
        };
    }

    public static IEnumerable<ApiDiscordOAuthUserResponse> FromOldList(IEnumerable<DiscordApiUserResponse> oldList,
        DataContext dataContext) => oldList.Select(d => FromOld(d, dataContext)!);
}