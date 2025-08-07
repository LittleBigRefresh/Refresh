using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

public class ByUserLevelCategory : GameLevelCategory
{
    internal ByUserLevelCategory() : base("byUser", "by", true)
    {
        // Technically this category can apply to any user, but since we fallback to the regular user this name & description still applies
        this.Name = "My Published Levels";
        this.Description = "Levels you've shared with the community!";
        this.IconHash = "g820625";
        this.FontAwesomeIcon = "user";
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count,
        DataContext dataContext,
        ResultFilterSettings levelFilterSettings, GameUser? user)
    {
        // Prefer username from query, but fallback to user passed into this category if it's missing
        string? username = context.QueryString["u"] ?? context.QueryString["username"];
        if (username != null) user = dataContext.Database.GetUserByUsername(username);

        if (user == null) return null;
        
        return dataContext.Database.GetLevelsByUser(user, count, skip, levelFilterSettings, dataContext.User);
    }
}