using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Core.Storage;
using Bunkum.Listener.Protocol;
using Refresh.Common.Constants;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.RateLimits.EndpointRateLimiting;
using Refresh.Core.Services;
using Refresh.Core.Types.Categories;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Relations;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;
using Refresh.Interfaces.Game.Endpoints.DataTypes.Response;
using Refresh.Interfaces.Game.Types.Levels;
using Refresh.Interfaces.Game.Types.Lists;

namespace Refresh.Interfaces.Game.Endpoints.Levels;

public class LevelEndpoints : EndpointGroup
{
    [GameEndpoint("slots/{route}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [EndpointRateLimit(EndpointBucketId.GameGetListOfLevels)]
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

        DatabaseResultList? results = categoryService.LevelCategories
            .FirstOrDefault(c => c.GameRoutes.Any(r => r.StartsWith(route)))?
            .Fetch(context, skip, count, dataContext, LevelFilterSettings.FromGameRequest(context, token.TokenGame), user);

        if (results == null) return null;
        
        IEnumerable<GameMinimalLevelResponse> slots = results.Levels?.Items.ToArray()
            .Select(l => GameMinimalLevelResponse.FromOld(l, dataContext)!) ?? [];

        // Insert playlists into slots if there are any. If they're not needed, the category itself will avoid looking them up.
        if (results.Playlists != null)
        {
            slots = slots.Concat(results.Playlists.Items.ToArray()
                .Select(p => GameMinimalLevelResponse.FromOld(p, dataContext)!));
        }
        
        IEnumerable<GameUserResponse> users = GameUserResponse.FromOldList(results.Users?.Items ?? [], dataContext);
        return new SerializedMinimalLevelList(slots, results.TotalItemsSum, results.NextPageIndexMax, users);
    }

    [GameEndpoint("slots/{route}/{username}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    [EndpointRateLimit(EndpointBucketId.GameGetListOfLevels)]
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

    // Route example: /lbp/slots/like/user/276&pageStart=1&pageSize=30
    // The syntax error in the query params (& instead of ?) makes Bunkum include them as part of the ID route param
    [GameEndpoint("slots/like/{slotType}/{id}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [EndpointRateLimit(EndpointBucketId.GameGetListOfLevels)]
    public Response GetLevelsLikeLevel(RequestContext context, DataContext dataContext, GameUser user, string slotType, string id)
    {
        string levelIdStr;
        int skip, count;

        // Get the level ID and the pagination params from the ID route parameter
        if (id.Contains('&'))
        {
            levelIdStr = id.Split('&')[0];

            string skipStr = "0";
            string countStr = "30";
            if (id.Contains("pageStart="))
            {
                skipStr = id.Split("pageStart=")[1];
            }
            if (id.Contains("pageSize="))
            {
                countStr = id.Split("pageSize=")[1];
            }

            (skip, count) = context.GetPageData(skipStr, countStr);
        }
        else
        {
            levelIdStr = id;
            (skip, count) = context.GetPageData();
        }

        bool idParsed = int.TryParse(levelIdStr, out int parsedId);
        if (!idParsed) return BadRequest;

        GameLevel? level = dataContext.Database.GetLevelByIdAndType(slotType, parsedId);
        if (level == null) return NotFound;

        // Simply take a random tag from the level and then get levels which use that tag
        IQueryable<TagLevelRelation> tags = dataContext.Database.GetTagRelationsForLevel(level);
        if (!tags.Any()) return new(new SerializedMinimalLevelList(), ContentType.Xml); // Return empty list if there are no tags for the level

        Tag tagToUse = tags.ElementAt(Random.Shared.Next(tags.Count())).Tag;
        DatabaseList<GameLevel> levels = dataContext.Database.GetLevelsByTag(count, skip, user, tagToUse, LevelFilterSettings.FromGameRequest(context, dataContext.Game));

        SerializedMinimalLevelList response = new(GameMinimalLevelResponse.FromOldList(levels.Items.ToArray(), dataContext)!, levels.TotalItems, levels.NextPageIndex);
        return new(response, ContentType.Xml);
    }

    [GameEndpoint("s/{slotType}/{id}", ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    [EndpointRateLimit(EndpointBucketId.GameGetSingleLevel)]
    public GameLevelResponse? LevelById(RequestContext context, GameDatabaseContext database, Token token,
        string slotType, int id,
        PlayNowService overrideService, DataContext dataContext)
    {
        // If the user has had a hash override in the past, and the level id they requested matches the level ID associated with that hash
        if (overrideService.GetLastHashOverrideForUser(token, out string hash) && GameLevel.LevelIdFromHash(hash) == id)
            // Return the hashed level info
            return GameLevelResponse.FromHash(hash, dataContext);
        
        return GameLevelResponse.FromOld(database.GetLevelByIdAndType(slotType, id), dataContext);
    }
    
    [GameEndpoint("slotList", ContentType.Xml)]
    [NullStatusCode(BadRequest)]
    [MinimumRole(GameUserRole.Restricted)]
    [EndpointRateLimit(EndpointBucketId.GameGetListOfLevels)]
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

    #region Quirk workarounds
    // Some LBP2 level routes don't appear under `/slots/`.
    // This is a list of endpoints to work around these - capturing all routes would break things.

    [GameEndpoint("slots", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [EndpointRateLimit(EndpointBucketId.GameGetListOfLevels)]
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
    [EndpointRateLimit(EndpointBucketId.GameGetListOfLevels)]
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