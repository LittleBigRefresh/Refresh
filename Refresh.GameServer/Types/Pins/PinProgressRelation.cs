using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Pins;

#nullable disable
public partial class PinProgressRelation : IRealmObject
{
    public long PinId { get; set; }
    public int Progress { get; set; }
    public GameUser Publisher { get; set; }

    public DateTimeOffset FirstPublished { get; set; }
    public DateTimeOffset LastUpdated { get; set; }

    public bool IsBeta { get; set; }
}