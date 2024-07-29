using Bunkum.Core;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Levels.Categories;

public class ByTagCategory : LevelCategory
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
        LevelFilterSettings levelFilterSettings, GameUser? user)
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