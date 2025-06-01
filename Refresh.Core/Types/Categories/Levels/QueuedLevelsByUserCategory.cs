using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

public class QueuedLevelsByUserCategory : GameLevelCategory
{
    internal QueuedLevelsByUserCategory() : base("queued", "lolcatftw", true)
    {
        this.Name = "My Queue";
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