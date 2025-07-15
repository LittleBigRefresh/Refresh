using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Listener.Protocol;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Core.Types.Categories;
using Refresh.Core.Types.Categories.Levels;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;
using Refresh.Interfaces.Game.Endpoints.DataTypes.Response;
using Refresh.Interfaces.Game.Types.Categories;
using Refresh.Interfaces.Game.Types.Levels;
using Refresh.Interfaces.Game.Types.Lists;

namespace Refresh.Interfaces.Game.Endpoints;

public class CategoryEndpoints : EndpointGroup
{
    [GameEndpoint("searches", ContentType.Xml)]
    [GameEndpoint("genres", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedCategoryList GetModernCategories(RequestContext context, CategoryService categoryService, DataContext dataContext, GameServerConfig config)
    {
        (int skip, int count) = context.GetPageData();

        IEnumerable<SerializedLevelCategory> levelCategories = categoryService.LevelCategories
            .Where(c => !c.Hidden)
            .Select(c => SerializedLevelCategory.FromLevelCategory(c, context, dataContext, 0, 1));
        
        IEnumerable<SerializedUserCategory> userCategories = config.PermitShowingOnlineUsers ? categoryService.UserCategories
            .Where(c => !c.Hidden)
            .Select(c => SerializedUserCategory.FromUserCategory(c, context, dataContext, 0, 1)) : [];

        IEnumerable<SerializedCategory> allCategories = [];
        allCategories = allCategories.Concat(levelCategories).Concat(userCategories);
        
        DatabaseList<SerializedCategory> paginatedCategories = new(allCategories, skip, count);

        SearchLevelCategory searchCategory = (SearchLevelCategory)categoryService.LevelCategories
            .First(c => c is SearchLevelCategory);
        
        return new SerializedCategoryList
        (
            paginatedCategories.Items, 
            searchCategory, 
            paginatedCategories.TotalItems
        );
    }

    [GameEndpoint("searches/levels/{apiRoute}", ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedCategoryResultsList? GetLevelsFromCategory(RequestContext context, CategoryService categories, GameUser user, 
        Token token, string apiRoute, DataContext dataContext)
    {
        (int skip, int count) = context.GetPageData();

        DatabaseList<GameLevel>? levels = categories.LevelCategories
            .FirstOrDefault(c => c.ApiRoute.StartsWith(apiRoute))?
            .Fetch(context, skip, count, dataContext, new LevelFilterSettings(context, token.TokenGame), user);
        
        if (levels == null) return null;
        
        return new SerializedCategoryResultsList
        (
            levels.Items.ToArray().Select(l => GameMinimalLevelResponse.FromOld(l, dataContext))!,
            levels.TotalItems,
            levels.NextPageIndex
        );
    }

    [GameEndpoint("searches/users/{apiRoute}", ContentType.Xml)]
    [NullStatusCode(NotFound)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedCategoryResultsList? GetUsersFromCategory(RequestContext context, CategoryService categories, GameUser user, 
        Token token, string apiRoute, DataContext dataContext, GameServerConfig config)
    {
        if (!config.PermitShowingOnlineUsers) return null;

        (int skip, int count) = context.GetPageData();

        DatabaseList<GameUser>? users = categories.UserCategories
            .FirstOrDefault(c => c.ApiRoute.StartsWith(apiRoute))?
            .Fetch(context, skip, count, dataContext, new LevelFilterSettings(context, token.TokenGame), user);
        
        if (users == null) return null;
        
        return new SerializedCategoryResultsList
        (
            users.Items.ToArray().Select(u => GameUserResponse.FromOld(u, dataContext))!,
            users.TotalItems,
            users.NextPageIndex
        );
    }
}