using Bunkum.Core;
using Refresh.Database.Query;
using Refresh.Database;
using Refresh.GameServer.Types.Data;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;

namespace Refresh.GameServer.Types.Categories.Levels;

public class AdventureCategory : GameLevelCategory
{
    public AdventureCategory() : base("adventure", [], false)
    {
        this.Name = "Adventures";
        this.Description = "Storylines and other big projects by the community.";
        this.FontAwesomeIcon = "book-bookmark";
        this.IconHash = "g820625";
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _)
    {
        return dataContext.Database.GetAdventureLevels(count, skip, dataContext.User, levelFilterSettings);
    }
}