using System.Net;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Levels.Categories;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game.Levels;

public class LevelEndpoints : EndpointGroup
{
    [GameEndpoint("slots/{route}", ContentType.Xml)]
    public GameMinimalLevelList GetLevels(RequestContext context, RealmDatabaseContext database, GameUser? user, string route) =>
        new(CategoryHandler.Categories
            .FirstOrDefault(c => c.GameRoute.StartsWith(route))?
            .Fetch(context, database, user)?
            .Select(GameMinimalLevel.FromGameLevel), database.GetTotalLevelCount()); // TODO: proper level count

    [GameEndpoint("slots/{route}/{username}", ContentType.Xml)]
    public GameMinimalLevelList GetLevelsWithPlayer(RequestContext context, RealmDatabaseContext database, string route, string username)
        => this.GetLevels(context, database, database.GetUserByUsername(username), route);

    [GameEndpoint("s/user/{idStr}", ContentType.Xml)]
    [NullStatusCode(HttpStatusCode.NotFound)]
    public GameLevel? LevelById(RequestContext context, RealmDatabaseContext database, string idStr)
    {
        int.TryParse(idStr, out int id);
        if (id == default) return null;
        
        return database.GetLevelById(id);
    }

    [GameEndpoint("slotList", ContentType.Xml)]
    [NullStatusCode(HttpStatusCode.BadRequest)]
    public GameLevelList? GetMultipleLevels(RequestContext context, RealmDatabaseContext database)
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

        return new GameLevelList
        {
            Items = levels,
            Total = levels.Count,
            NextPageStart = 0,
        };
    }

    #region Quirk workarounds
    // Some LBP2 level routes don't appear under `/slots/`.
    // This is a list of endpoints to work around these - capturing all routes would break things.

    [GameEndpoint("slots", ContentType.Xml)]
    public GameMinimalLevelList NewestLevels(RequestContext context, RealmDatabaseContext database, GameUser? user) 
        => this.GetLevels(context, database, user, "newest");

    [GameEndpoint("favouriteSlots/{username}", ContentType.Xml)]
    public GameMinimalFavouriteLevelList FavouriteLevels(RequestContext context, RealmDatabaseContext database, string username)
    {
        GameMinimalLevelList levels = this.GetLevels(context, database, database.GetUserByUsername(username), "favouriteSlots");
        return new GameMinimalFavouriteLevelList(levels);
    }

    #endregion
}