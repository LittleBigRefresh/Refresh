using Realms;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Relations;
#nullable disable

public class FavouriteLevelRelation : RealmObject
{
    public GameLevel Level { get; set; }
    public GameUser User { get; set; }
}