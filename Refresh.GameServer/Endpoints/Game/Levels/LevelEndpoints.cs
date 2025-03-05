using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Bunkum.Listener.Protocol;
using Refresh.Common.Constants;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Levels.Categories;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Playlists;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game.Levels;

public class LevelEndpoints : EndpointGroup
{
    [GameEndpoint("slots/{route}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedMinimalLevelList? GetLevels(RequestContext context,
        GameDatabaseContext database,
        CategoryService categoryService,
        PlayNowService overrideService,
        GameUser user,
        Token token,
        DataContext dataContext,
        string route)
    {
        if (overrideService.UserHasOverrides(user))
        {
            List<GameMinimalLevelResponse> overrides = [];
            
            if (overrideService.GetIdOverridesForUser(token, database, out IEnumerable<GameLevel> levelOverrides))
                overrides.AddRange(levelOverrides.Select(l => GameMinimalLevelResponse.FromOld(l, dataContext))!);
            
            if (overrideService.GetHashOverrideForUser(token, out string hashOverride))
                overrides.Add(GameMinimalLevelResponse.FromHash(hashOverride, dataContext));
            
            return new SerializedMinimalLevelList(overrides, overrides.Count, overrides.Count);
        }
        
        // If we are getting the levels by a user, and that user is "!Hashed", then we pull that user's overrides
        if (route == "by" 
            && (context.QueryString.Get("u") == SystemUsers.HashedUserName || user.Username == SystemUsers.HashedUserName) 
            && overrideService.GetLastHashOverrideForUser(token, out string hash))
        {
            return new SerializedMinimalLevelList
            {
                Total = 1,
                NextPageStart = 1,
                Items = [GameMinimalLevelResponse.FromHash(hash, dataContext)],
            };
        }
        
        (int skip, int count) = context.GetPageData();

        DatabaseList<GameLevel>? levels = categoryService.Categories
            .FirstOrDefault(c => c.GameRoutes.Any(r => r.StartsWith(route)))?
            .Fetch(context, skip, count, dataContext, new LevelFilterSettings(context, token.TokenGame), user);

        if (levels == null) return null;
        
        IEnumerable<GameMinimalLevelResponse> category = levels.Items
            .Select(l => GameMinimalLevelResponse.FromOld(l, dataContext)!);

        int injectedAmount = 0;
        
        // Special case the `by` route for LBP1 requests, to inject the user's playlist info
        if (route == "by" && dataContext.Game == TokenGame.LittleBigPlanet1)
        {
            // Get the requested user's root playlist
            GamePlaylist? playlist = database.GetUserByUsername(context.QueryString.Get("u"))?.RootPlaylist;

            // If it was found, inject it into the response info
            if (playlist != null)
            {
                // TODO: with postgres this can be IQueryable
                List<GamePlaylist> playlists = database.GetPlaylistsInPlaylist(playlist).ToList();
                
                category = GameMinimalLevelResponse.FromOldList(playlists, dataContext).Concat(category);
                // While this does technically return more slot results than the game is expecting,
                // because we tell the game exactly what the "next page index" is (its not based on count sent),
                // pagination still seems to work perfectly fine in LBP1!
                // The injected items are basically just fake slots which "follow" the current page.
                injectedAmount += playlists.Count;
            }
        }   

        return new SerializedMinimalLevelList(category, levels.TotalItems + injectedAmount, skip + count);
    }

    [GameEndpoint("slots/{route}/{username}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedMinimalLevelList? GetLevelsWithPlayer(RequestContext context,
        GameDatabaseContext database,
        CategoryService categories,
        PlayNowService overrideService,
        Token token,
        DataContext dataContext,
        string route,
        string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return null;
        
        return this.GetLevels(context, database, categories, overrideService, user, token, dataContext, route);
    }

    [GameEndpoint("s/{slotType}/{id}", ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public GameLevelResponse? LevelById(RequestContext context, GameDatabaseContext database, Token token,
        string slotType, int id,
        PlayNowService overrideService, DataContext dataContext)
    {
        // If the user has had a hash override in the past, and the level id they requested matches the level ID associated with that hash
        if (overrideService.GetLastHashOverrideForUser(token, out string hash) && GameLevelResponse.LevelIdFromHash(hash) == id)
            // Return the hashed level info
            return GameLevelResponse.FromHash(hash, dataContext);
        
        return GameLevelResponse.FromOld(database.GetLevelByIdAndType(slotType, id), dataContext);
    }
    
    [GameEndpoint("slotList", ContentType.Xml)]
    [NullStatusCode(BadRequest)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedLevelList? GetMultipleLevels(RequestContext context, GameDatabaseContext database,
        GameUser user, Token token, DataContext dataContext)
    {
        string[]? levelIds = context.QueryString.GetValues("s");
        if (levelIds == null) return null;

        List<GameLevelResponse> levels = [];
        
        foreach (string levelIdStr in levelIds)
        {
            // Sometimes, in playlists for example, LBP3 refers to developer levels by using their level (not story) id
            // and prepending a 'd' to it.
            // We need to remove it in order to be able to parse the id and get the level.
            // If parsing fails anyway, skip over the level id and continue with the next one.
            if (!int.TryParse(levelIdStr.StartsWith('d') ? levelIdStr[1..] : levelIdStr, out int levelId)) continue;
            GameLevel? level = database.GetLevelById(levelId);

            if (level == null) continue;
            
            levels.Add(GameLevelResponse.FromOld(level, dataContext)!);
        }

        return new SerializedLevelList
        {
            Items = levels,
            Total = levels.Count,
            NextPageStart = 0,
        };
    }

    [GameEndpoint("searches", ContentType.Xml)]
    [GameEndpoint("genres", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedCategoryList GetModernCategories(RequestContext context, CategoryService categoryService, DataContext dataContext)
    {
        (int skip, int count) = context.GetPageData();

        IEnumerable<SerializedCategory> categories = categoryService.Categories
            .Where(c => !c.Hidden)
            .Select(c => SerializedCategory.FromLevelCategory(c, context, dataContext, 0, 1))
            .ToList();

        int total = categories.Count();

        categories = categories.Skip(skip).Take(count);

        SearchLevelCategory searchCategory = (SearchLevelCategory)categoryService.Categories
            .First(c => c is SearchLevelCategory);
        
        return new SerializedCategoryList(categories, searchCategory, total);
    }

    [GameEndpoint("searches/{apiRoute}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedMinimalLevelResultsList GetLevelsFromCategory(RequestContext context,
        CategoryService categories, GameUser user, Token token, string apiRoute, DataContext dataContext)
    {
        (int skip, int count) = context.GetPageData();

        DatabaseList<GameLevel>? levels = categories.Categories
            .FirstOrDefault(c => c.ApiRoute.StartsWith(apiRoute))?
            .Fetch(context, skip, count, dataContext, new LevelFilterSettings(context, token.TokenGame), user);
        
        return new SerializedMinimalLevelResultsList(levels?.Items
            .Select(l => GameMinimalLevelResponse.FromOld(l, dataContext))!, levels?.TotalItems ?? 0, skip + count);
    }

    #region Quirk workarounds
    // Some LBP2 level routes don't appear under `/slots/`.
    // This is a list of endpoints to work around these - capturing all routes would break things.

    [GameEndpoint("slots", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedMinimalLevelList? NewestLevels(RequestContext context,
        GameDatabaseContext database,
        CategoryService categories,
        MatchService matchService,
        PlayNowService overrideService,
        GameUser user,
        IDataStore dataStore,
        Token token,
        DataContext dataContext) 
        => this.GetLevels(context, database, categories, overrideService, user, token, dataContext, "newest");

    [GameEndpoint("favouriteSlots/{username}", ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedMinimalFavouriteLevelList? FavouriteLevels(RequestContext context,
        GameDatabaseContext database,
        CategoryService categories,
        MatchService matchService,
        PlayNowService overrideService,
        Token token,
        IDataStore dataStore,
        DataContext dataContext,
        string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return null;
        
        SerializedMinimalLevelList? levels = this.GetLevels(context, database, categories, overrideService, user, token, dataContext, "favouriteSlots");
        
        return new SerializedMinimalFavouriteLevelList(levels);
    }

    #endregion
}