using Realms;

namespace Refresh.GameServer.Types.UserData;

#nullable disable

public partial class GameIpVerificationRequest : IEmbeddedObject
{
    public string IpAddress { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}