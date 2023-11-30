using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Listener.Protocol;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Levels.Categories;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game.Levels;

public class LevelEndpoints : EndpointGroup
{
    [GameEndpoint("slots/{route}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedMinimalLevelList? GetLevels(RequestContext context,
        IGameDatabaseContext database,
        CategoryService categoryService,
        MatchService matchService,
        LevelListOverrideService overrideService,
        GameUser user,
        Token token,
        string route)
    {
        if (overrideService.UserHasOverrides(user))
        {
            List<GameMinimalLevelResponse> overrides = overrideService.GetOverridesForUser(user, database)
                .Select(l => GameMinimalLevelResponse.FromOldWithExtraData(l, matchService))
                .ToList()!;
            
            return new SerializedMinimalLevelList(overrides, overrides.Count);
        }
        
        (int skip, int count) = context.GetPageData();

        DatabaseList<GameLevel>? levels = categoryService.Categories
            .FirstOrDefault(c => c.GameRoutes.Any(r => r.StartsWith(route)))?
            .Fetch(context, skip, count, matchService, database, user, new LevelFilterSettings(context, token.TokenGame));

        if (levels == null) return null;
        
        IEnumerable<GameMinimalLevelResponse> category = levels.Items
            .Select(l => GameMinimalLevelResponse.FromOldWithExtraData(l, matchService))!;
        
        return new SerializedMinimalLevelList(category, levels.TotalItems);
    }

    [GameEndpoint("slots/{route}/{username}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedMinimalLevelList? GetLevelsWithPlayer(RequestContext context,
        IGameDatabaseContext database,
        CategoryService categories,
        MatchService matchService,
        LevelListOverrideService overrideService,
        Token token,
        string route,
        string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return null;
        
        return this.GetLevels(context, database, categories, matchService, overrideService, user, token, route);
    }

    [GameEndpoint("s/user/{id}", ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public GameLevelResponse? LevelById(RequestContext context, IGameDatabaseContext database, MatchService matchService, GameUser user, int id)
        => GameLevelResponse.FromOldWithExtraData(database.GetLevelById(id), database, matchService, user);

    [GameEndpoint("slotList", ContentType.Xml)]
    [NullStatusCode(BadRequest)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedLevelList? GetMultipleLevels(RequestContext context, IGameDatabaseContext database, MatchService matchService, GameUser user)
    {
        string[]? levelIds = context.QueryString.GetValues("s");
        if (levelIds == null) return null;

        List<GameLevelResponse> levels = new();
        
        foreach (string levelIdStr in levelIds)
        {
            if (!int.TryParse(levelIdStr, out int levelId)) return null;
            GameLevel? level = database.GetLevelById(levelId);

            if (level == null) continue;
            
            levels.Add(GameLevelResponse.FromOldWithExtraData(level, database, matchService, user)!);
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
    public SerializedCategoryList GetModernCategories(RequestContext context, IGameDatabaseContext database, CategoryService categoryService, MatchService matchService, GameUser user, Token token)
    {
        (int skip, int count) = context.GetPageData();

        IEnumerable<SerializedCategory> categories = categoryService.Categories
            .Where(c => !c.Hidden)
            .Select(c => SerializedCategory.FromLevelCategory(c, context, database, user, token, matchService, 0, 1))
            .Where(c => c.Levels.Total > 0)
            .ToList();

        int total = categories.Count();

        categories = categories.Skip(skip).Take(count);
        
        return new SerializedCategoryList(categories, total);
    }

    [GameEndpoint("searches/{apiRoute}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedMinimalLevelResultsList GetLevelsFromCategory(RequestContext context, IGameDatabaseContext database, CategoryService categories, MatchService matchService, GameUser user, Token token, string apiRoute)
    {
        (int skip, int count) = context.GetPageData();

        DatabaseList<GameLevel>? levels = categories.Categories
            .FirstOrDefault(c => c.ApiRoute.StartsWith(apiRoute))?
            .Fetch(context, skip, count, matchService, database, user, new LevelFilterSettings(context, token.TokenGame));
        
        return new SerializedMinimalLevelResultsList(levels?.Items
            .Select(l => GameMinimalLevelResponse.FromOldWithExtraData(l, matchService))!, levels?.TotalItems ?? 0);
    }

    #region Quirk workarounds
    // Some LBP2 level routes don't appear under `/slots/`.
    // This is a list of endpoints to work around these - capturing all routes would break things.

    [GameEndpoint("slots", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedMinimalLevelList? NewestLevels(RequestContext context,
        IGameDatabaseContext database,
        CategoryService categories,
        MatchService matchService,
        LevelListOverrideService overrideService,
        GameUser user,
        Token token) 
        => this.GetLevels(context, database, categories, matchService, overrideService, user, token, "newest");

    [GameEndpoint("favouriteSlots/{username}", ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedMinimalFavouriteLevelList? FavouriteLevels(RequestContext context,
        IGameDatabaseContext database,
        CategoryService categories,
        MatchService matchService,
        LevelListOverrideService overrideService,
        Token token,
        string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return null;
        
        SerializedMinimalLevelList? levels = this.GetLevels(context, database, categories, matchService, overrideService, user, token, "favouriteSlots");
        
        return new SerializedMinimalFavouriteLevelList(levels);
    }

    #endregion
}