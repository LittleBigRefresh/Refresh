using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
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
        GameDatabaseContext database,
        CategoryService categoryService,
        MatchService matchService,
        GameUser user,
        Token token,
        string route)
    {
        (int skip, int count) = context.GetPageData();

        DatabaseList<GameLevel>? levels = categoryService.Categories
            .FirstOrDefault(c => c.GameRoute.StartsWith(route))?
            .Fetch(context, skip, count, matchService, database, user, token.TokenGame);

        if (levels == null) return null;
        
        IEnumerable<GameMinimalLevelResponse> category = levels.Items
            .Select(l => GameMinimalLevelResponse.FromOldWithExtraData(l, matchService))!;
        
        return new SerializedMinimalLevelList(category, levels.TotalItems);
    }

    [GameEndpoint("slots/{route}/{username}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedMinimalLevelList? GetLevelsWithPlayer(RequestContext context, GameDatabaseContext database, CategoryService categories, MatchService matchService, Token token, string route, string username)
        => this.GetLevels(context, database, categories, matchService, database.GetUserByUsername(username), token, route);

    [GameEndpoint("s/user/{id}", ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public GameLevelResponse? LevelById(RequestContext context, GameDatabaseContext database, MatchService matchService, GameUser user, int id)
        => GameLevelResponse.FromOldWithExtraData(database.GetLevelById(id), database, matchService, user);

    [GameEndpoint("slotList", ContentType.Xml)]
    [NullStatusCode(BadRequest)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedLevelList? GetMultipleLevels(RequestContext context, GameDatabaseContext database, MatchService matchService, GameUser user)
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
    public SerializedCategoryList GetModernCategories(RequestContext context, GameDatabaseContext database, CategoryService categoryService, MatchService matchService, GameUser user, Token token)
    {
        IEnumerable<SerializedCategory> categories = categoryService.Categories
            .Where(c => c is not SearchLevelCategory)
            .Take(5)
            .Select(c => SerializedCategory.FromLevelCategory(c, context, database, user, token, matchService, 0, 1));
        
        return new SerializedCategoryList(categories, categoryService);
    }

    [GameEndpoint("searches/{apiRoute}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedMinimalLevelList GetLevelsFromCategory(RequestContext context, GameDatabaseContext database, CategoryService categories, MatchService matchService, GameUser user, Token token, string apiRoute)
    {
        (int skip, int count) = context.GetPageData();

        DatabaseList<GameLevel>? levels = categories.Categories
            .FirstOrDefault(c => c.ApiRoute.StartsWith(apiRoute))?
            .Fetch(context, skip, count, matchService, database, user, token.TokenGame);
        
        return new SerializedMinimalLevelResultsList(levels?.Items
            .Select(l => GameMinimalLevelResponse.FromOldWithExtraData(l, matchService))!, levels?.TotalItems ?? 0);
    }

    #region Quirk workarounds
    // Some LBP2 level routes don't appear under `/slots/`.
    // This is a list of endpoints to work around these - capturing all routes would break things.

    [GameEndpoint("slots", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedMinimalLevelList? NewestLevels(RequestContext context, GameDatabaseContext database, CategoryService categories, MatchService matchService, GameUser user, Token token) 
        => this.GetLevels(context, database, categories, matchService, user, token, "newest");

    [GameEndpoint("favouriteSlots/{username}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedMinimalFavouriteLevelList? FavouriteLevels(RequestContext context, GameDatabaseContext database, CategoryService categories, MatchService matchService, Token token, string username)
    {
        SerializedMinimalLevelList? levels = this.GetLevels(context, database, categories, matchService, database.GetUserByUsername(username), token, "favouriteSlots");
        if (levels == null) return null;
        
        return new SerializedMinimalFavouriteLevelList(levels);
    }

    #endregion
}