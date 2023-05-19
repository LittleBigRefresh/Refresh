using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Levels.Categories;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game.Levels;

public class LevelEndpoints : EndpointGroup
{
    [GameEndpoint("slots/{route}", ContentType.Xml)]
    public SerializedMinimalLevelList? GetLevels(RequestContext context, GameDatabaseContext database, CategoryService categories, GameUser? user, string route)
    {
        (int skip, int count) = context.GetPageData();

        IEnumerable<GameMinimalLevel>? category = categories.Categories
            .FirstOrDefault(c => c.GameRoute.StartsWith(route))?
            .Fetch(context, skip, count, database, user)?
            .Select(GameMinimalLevel.FromGameLevel);

        if (category == null) return null;
        
        return new SerializedMinimalLevelList(category, database.GetTotalLevelCount());
        // TODO: proper level count
    }

    [GameEndpoint("slots/{route}/{username}", ContentType.Xml)]
    public SerializedMinimalLevelList? GetLevelsWithPlayer(RequestContext context, GameDatabaseContext database, CategoryService categories, string route, string username)
        => this.GetLevels(context, database, categories, database.GetUserByUsername(username), route);

    [GameEndpoint("s/user/{idStr}", ContentType.Xml)]
    [NullStatusCode(NotFound)]
    public GameLevel? LevelById(RequestContext context, GameDatabaseContext database, string idStr)
    {
        int.TryParse(idStr, out int id);
        if (id == default) return null;
        
        return database.GetLevelById(id);
    }

    [GameEndpoint("slotList", ContentType.Xml)]
    [NullStatusCode(BadRequest)]
    public SerializedLevelList? GetMultipleLevels(RequestContext context, GameDatabaseContext database)
    {
        string[]? levelIds = context.QueryString.GetValues("s");
        if (levelIds == null) return null;

        List<GameLevel> levels = new();
        
        foreach (string levelIdStr in levelIds)
        {
            if (!int.TryParse(levelIdStr, out int levelId)) return null;
            GameLevel? level = database.GetLevelById(levelId);

            if (level == null) continue;
            
            level.PrepareForSerialization();
            levels.Add(level);
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
    public SerializedCategoryList GetModernCategories(RequestContext context, GameDatabaseContext database, CategoryService categoryService, GameUser user)
    {
        IEnumerable<SerializedCategory> categories = categoryService.Categories
            .Where(c => c is not SearchLevelCategory)
            .Take(5)
            .Select(c => SerializedCategory.FromLevelCategory(c, context, database, user, 0, 1));
        
        return new SerializedCategoryList(categories, categoryService);
    }

    [GameEndpoint("searches/{apiRoute}", ContentType.Xml)]
    public SerializedMinimalLevelList GetLevelsFromCategory(RequestContext context, GameDatabaseContext database, CategoryService categories, GameUser? user, string apiRoute)
    {
        (int skip, int count) = context.GetPageData();
        
        return new SerializedMinimalLevelResultsList(categories.Categories
            .FirstOrDefault(c => c.ApiRoute.StartsWith(apiRoute))?
            .Fetch(context, skip, count, database, user)?
            .Select(GameMinimalLevel.FromGameLevel), database.GetTotalLevelCount());
    }

    #region Quirk workarounds
    // Some LBP2 level routes don't appear under `/slots/`.
    // This is a list of endpoints to work around these - capturing all routes would break things.

    [GameEndpoint("slots", ContentType.Xml)]
    public SerializedMinimalLevelList? NewestLevels(RequestContext context, GameDatabaseContext database, CategoryService categories, GameUser? user) 
        => this.GetLevels(context, database, categories, user, "newest");

    [GameEndpoint("favouriteSlots/{username}", ContentType.Xml)]
    public SerializedMinimalFavouriteLevelList? FavouriteLevels(RequestContext context, GameDatabaseContext database, CategoryService categories, string username)
    {
        SerializedMinimalLevelList? levels = this.GetLevels(context, database, categories, database.GetUserByUsername(username), "favouriteSlots");
        if (levels == null) return null;
        
        return new SerializedMinimalFavouriteLevelList(levels);
    }

    #endregion
}