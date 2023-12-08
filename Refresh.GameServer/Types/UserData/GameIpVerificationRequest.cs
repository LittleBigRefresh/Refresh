using Microsoft.EntityFrameworkCore;
using Realms;

namespace Refresh.GameServer.Types.UserData;

#nullable disable

[Keyless] // TODO: AGONY
public partial class GameIpVerificationRequest : IEmbeddedObject
{
    public string IpAddress { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}