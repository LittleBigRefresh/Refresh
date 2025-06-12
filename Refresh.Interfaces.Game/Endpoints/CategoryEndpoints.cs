using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Listener.Protocol;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Types.Categories;
using Refresh.Core.Types.Categories.Levels;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;
using Refresh.Interfaces.Game.Types.Categories;
using Refresh.Interfaces.Game.Types.Levels;
using Refresh.Interfaces.Game.Types.Lists;

namespace Refresh.Interfaces.Game.Endpoints;

public class CategoryEndpoints : EndpointGroup
{
    [GameEndpoint("searches", ContentType.Xml)]
    [GameEndpoint("genres", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedCategoryList GetModernCategories(RequestContext context, CategoryService categoryService, DataContext dataContext)
    {
        (int skip, int count) = context.GetPageData();

        IEnumerable<SerializedLevelCategory> levelCategories = categoryService.LevelCategories
            .Where(c => !c.Hidden)
            .Select(c => SerializedLevelCategory.FromLevelCategory(c, context, dataContext, 0, 1));

        DatabaseList<SerializedCategory> serializedCategories = new(levelCategories, skip, count);

        SearchLevelCategory searchCategory = (SearchLevelCategory)categoryService.LevelCategories
            .First(c => c is SearchLevelCategory);
        
        return new SerializedCategoryList
        (
            serializedCategories.Items, 
            searchCategory, 
            serializedCategories.TotalItems
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
}