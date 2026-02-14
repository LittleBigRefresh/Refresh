using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Users;

public class NewestUsersCategory : GameCategory
{
    public NewestUsersCategory() : base("newest", [], false)
    {
        this.Name = "Newest Players";
        this.Description = "Our newest server users.";
        this.FontAwesomeIcon = "user-plus";
        this.IconHash = "g820602";
        this.PrimaryResultType = ResultType.User;
    }

    public override DatabaseResultList? Fetch(RequestContext context, int skip, int count, DataContext dataContext, 
        LevelFilterSettings levelFilterSettings, GameUser? _)
    {
        return new(dataContext.Database.GetUsers(count, skip));
    }
}