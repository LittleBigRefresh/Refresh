using Microsoft.EntityFrameworkCore;
using Realms;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Relations;

#nullable disable

[Keyless] // TODO: AGONY
public partial class QueueLevelRelation : IRealmObject
{
    public GameLevel Level { get; set; }
    public GameUser User { get; set; }
}