using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

public class ByTagCategory : GameLevelCategory
{
    internal ByTagCategory() : base("tag", "tag", false)
    {
        // Technically this category can apply to any user, but since we fallback to the regular user this name & description still applies
        this.Name = "Tag Search";
        this.Description = "Search for levels using tags given by users like you!";
        this.IconHash = "g820605";
        this.FontAwesomeIcon = "tag";
        this.Hidden = true; // The by-tag category is not meant to be shown, as it requires a special implementation on all frontends
    }

    public override DatabaseList<GameLevel>? Fetch(RequestContext context, int skip, int count,
        DataContext dataContext,
        ResultFilterSettings levelFilterSettings, GameUser? user)
    {
        string? tagStr = context.QueryString["tag"];

        if (tagStr == null) 
            return null;

        Tag? tag = TagExtensions.FromLbpString(tagStr);

        if (tag == null)
            return null;

        return dataContext.Database.GetLevelsByTag(count, skip, user, tag.Value, levelFilterSettings);
    }
}