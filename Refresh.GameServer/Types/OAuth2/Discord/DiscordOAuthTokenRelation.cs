using MongoDB.Bson;
using Realms;

namespace Refresh.GameServer.Types.OAuth2.Discord;

public partial class DiscordOAuthTokenRelation : IRealmObject
{
    [PrimaryKey]
    public ObjectId UserId { get; set; }
    
    /// <summary>
    /// The user's access token
    /// </summary>
    public string? AccessToken { get; set; }
    /// <summary>
    /// The time the access token gets revoked
    /// </summary>
    public DateTimeOffset AccessTokenRevocationTime { get; set; }
    /// <summary>
    /// The refresh token used to get a new access token
    /// </summary>
    public string? RefreshToken { get; set; }
}