using Realms;

namespace Refresh.GameServer.Types.UserData;

#nullable disable

public partial class GameIpVerificationRequest : IRealmObject
{
    public GameUser User { get; set; }
    
    public string IpAddress { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}