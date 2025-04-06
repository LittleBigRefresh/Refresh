
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Listener.Protocol;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Categories;
using Refresh.GameServer.Types.Categories.Levels;
using Refresh.GameServer.Types.Categories.Playlists;
using Refresh.GameServer.Types.Categories.Users;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Lists.Results;
using Refresh.GameServer.Types.Playlists;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game.Levels;

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

        IEnumerable<SerializedPlaylistCategory> playlistCategories = categoryService.PlaylistCategories
            .Where(c => !c.Hidden)
            .Select(c => SerializedPlaylistCategory.FromPlaylistCategory(c, context, dataContext, 0, 1));

        IEnumerable<SerializedUserCategory> userCategories = categoryService.UserCategories
            .Where(c => !c.Hidden)
            .Select(c => SerializedUserCategory.FromUserCategory(c, context, dataContext, 0, 1));

        DatabaseList<SerializedCategory> serializedCategories = new
        (
            [.. levelCategories, .. playlistCategories, .. userCategories],
            skip,
            count
        );

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
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedMinimalLevelResultsList GetLevelsFromCategory(RequestContext context,
        CategoryService categories, GameUser user, Token token, string apiRoute, DataContext dataContext)
    {
        (int skip, int count) = context.GetPageData();

        DatabaseList<GameLevel>? levels = categories.LevelCategories
            .FirstOrDefault(c => c.ApiRoute.StartsWith(apiRoute))?
            .Fetch(context, skip, count, dataContext, new LevelFilterSettings(context, token.TokenGame), user);
        
        return new SerializedMinimalLevelResultsList(levels?.Items
            .Select(l => GameMinimalLevelResponse.FromOld(l, dataContext))!, levels?.TotalItems ?? 0, skip + count);
    }

    [GameEndpoint("searches/playlists/{apiRoute}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedLbp3PlaylistResultsList GetPlaylistsFromCategory(RequestContext context,
        CategoryService categories, GameUser user, Token token, string apiRoute, DataContext dataContext)
    {
        (int skip, int count) = context.GetPageData();

        DatabaseList<GamePlaylist>? playlists = categories.PlaylistCategories
            .FirstOrDefault(c => c.ApiRoute.StartsWith(apiRoute))?
            .Fetch(context, skip, count, dataContext, new LevelFilterSettings(context, token.TokenGame), user);
        
        return new SerializedLbp3PlaylistResultsList(playlists?.Items
            .Select(p => SerializedLbp3Playlist.FromOld(p, dataContext))!, playlists?.TotalItems ?? 0, skip + count);
    }

    [GameEndpoint("searches/users/{apiRoute}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedUserResultsList GetUsersFromCategory(RequestContext context,
        CategoryService categories, GameUser user, Token token, string apiRoute, DataContext dataContext)
    {
        (int skip, int count) = context.GetPageData();

        DatabaseList<GameUser>? users = categories.UserCategories
            .FirstOrDefault(c => c.ApiRoute.StartsWith(apiRoute))?
            .Fetch(context, skip, count, dataContext, new LevelFilterSettings(context, token.TokenGame), user);
        
        return new SerializedUserResultsList(users?.Items
            .Select(u => GameUserResponse.FromOld(u, dataContext))!, users?.TotalItems ?? 0, skip + count);
    }
}