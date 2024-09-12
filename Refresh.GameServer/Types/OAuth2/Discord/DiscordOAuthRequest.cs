using MongoDB.Bson;
using Realms;

namespace Refresh.GameServer.Types.OAuth2.Discord;

public partial class DiscordOAuthRequest : IRealmObject
{
    public ObjectId UserId { get; set; }
    public string State { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}