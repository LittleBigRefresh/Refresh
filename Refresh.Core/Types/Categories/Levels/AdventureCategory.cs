using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

public class AdventureCategory : GameCategory
{
    public AdventureCategory() : base("adventure", [], false)
    {
        this.Name = "Adventures";
        this.Description = "Storylines and other big projects by the community.";
        this.FontAwesomeIcon = "book-bookmark";
        this.IconHash = "g820625";
        this.PrimaryResultType = ResultType.Level; // Adventures are GameLevels
    }

    public override DatabaseResultList? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _)
    {
        return new(dataContext.Database.GetAdventureLevels(count, skip, dataContext.User, levelFilterSettings));
    }
}