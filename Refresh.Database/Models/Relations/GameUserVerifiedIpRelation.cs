using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Relations;

#nullable disable

public partial class GameUserVerifiedIpRelation : IRealmObject
{
    public GameUser User { get; set; }
    public string IpAddress { get; set; }
    public DateTimeOffset VerifiedAt { get; set; }
}