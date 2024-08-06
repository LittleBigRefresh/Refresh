using Realms;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Relations;

#nullable disable

public partial class PlayLevelRelation : IRealmObject
{
    public GameLevel Level { get; set; }
    public GameUser User { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public int Count { get; set; }
}