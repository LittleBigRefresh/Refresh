using Bunkum.Core;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Categories.Levels;

public class QueuedLevelsByUserCategory : GameLevelCategory
{
    internal QueuedLevelsByUserCategory() : base("queued", "lolcatftw", true)
    {
        this.Name = "Your Queue";
        this.Description = "Levels you'd like to play!";
        this.FontAwesomeIcon = "bell";
        this.IconHash = "g820614";
    }
    
    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? user)
    {
        if (user == null) return null;
        return dataContext.Database.GetLevelsQueuedByUser(user, count, skip, levelFilterSettings, dataContext.User);
    }
}