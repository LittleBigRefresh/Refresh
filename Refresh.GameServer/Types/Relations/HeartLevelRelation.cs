using Realms;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Relations;

#nullable disable

public class HeartLevelRelation : RealmObject
{
    public HeartLevelRelation()
    {}

    public HeartLevelRelation(GameLevel level, GameUser user)
    {
        this.Level = level;
        this.User = user;
    }
    
    public GameLevel Level { get; set; }
    public GameUser User { get; set; }
}