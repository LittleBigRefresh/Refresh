using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Users;

public class NewestUsersCategory : GameUserCategory
{
    public NewestUsersCategory() : base("newest", [], false)
    {
        this.Name = "Newest users";
        this.Description = "Our newest server users.";
        this.FontAwesomeIcon = "user-plus";
        this.IconHash = "g820625";
    }

    public override DatabaseList<GameUser>? Fetch(RequestContext context, int skip, int count, DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _)
    {
        return dataContext.Database.GetUsers(count, skip);
    }
}