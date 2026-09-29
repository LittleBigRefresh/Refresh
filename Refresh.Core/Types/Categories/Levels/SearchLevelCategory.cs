using Bunkum.Core;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;

namespace Refresh.Core.Types.Categories.Levels;

// Since the games only want one search category to return all entity types, we will have to do so for them.
// Additionally, LBP1/2 use /slots/search for searching, so it has to be a level category.
// For the API however, we should keep searching for different entity types in separate categories for now.
// This is due to current limitations in APIv3 spec (level categories may only return levels, user categories only users etc.)
// I couldn't find a way to have one search category for API without having something very hacky or messy in general,
// and separate categories should be enough for now. I really don't feel like bothering with this anymore,
// so if we really do want one for all types eventually, we should think of it when that time comes.
// Doing it like this (for now) will keep us from adding new stuff to the spec we might regret later on anyway.
public class SearchLevelCategory : GameCategory
{
    internal SearchLevelCategory() : base("search", "search", false)
    {
        this.Name = "Search";
        this.Description = "Search for levels by name and description.";
        this.FontAwesomeIcon = "magnifying-glass";
        // no icon for now, too lazy to find
        this.Hidden = true; // The search category is not meant to be shown, as it requires a special implementation on all frontends
        this.PrimaryResultType = ResultType.Level;
    }

    public override DatabaseResultList? Fetch(RequestContext context, int skip, int count,
        DataContext dataContext,
        LevelFilterSettings levelFilterSettings, GameUser? _)
    {
        string? query = context.QueryString["query"]
                        ?? context.QueryString["textFilter"]; // LBP3 sends this instead of query
        if (query == null) return null;

        DatabaseList<GameLevel>? levels = !levelFilterSettings.DisplayLevels
            ? null
            : dataContext.Database.SearchForLevels(count, skip, dataContext.User, levelFilterSettings, query);
        
        // TODO Allow specifying custom params in queries, so users could filter entity types in LBP1/2 as well.
        // TODO Also allow users to explicitly tell us to not search in name or description.
        //
        // For some reason, LBP1 shows user results as large instead of small polaroids.
        // We should return less users than requested there to not make the polaroids/badges too messy.
        // We also need to modify skip for this so we don't skip over users when paginating.
        int userCount = count;
        int userSkip = skip;
        if (levelFilterSettings.GameVersion == TokenGame.LittleBigPlanet1)
        {
            userCount /= 3;
            userSkip /= 3;
        }
        
        DatabaseList<GameUser>? users = !levelFilterSettings.DisplayUsers || context.IsApi() // won't be able to return users anyway there
            ? null
            : dataContext.Database.SearchForUsers(userCount, userSkip, query);

        if (users != null && users.NextPageIndex > 0)
        {
            // If there is a next page for users, fix its NextPageIndex to use the original count/skip
            // instead of our fake ones. This way, if NextPageIndex for users is larger than that of levels,
            // our slots endpoint will know to return this, and the game will correctly request the next page,
            // of which we will divide count and skip again.
            users.NextPageIndex = skip + count + 1;
        }
        
        // TODO also allow searching and returning playlists
        
        return new(levels, users, null);
    }
}