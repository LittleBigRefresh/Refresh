using Bunkum.Core;
using Refresh.Core.Types.Categories.Levels;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Query;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Levels;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Categories;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiLevelCategoryResponse : ApiCategoryResponse, IApiResponse, IDataConvertableFrom<ApiLevelCategoryResponse, GameLevelCategory>
{
    public required ApiGameLevelResponse? PreviewLevel { get; set; }
    
    public static ApiLevelCategoryResponse? FromOld(GameLevelCategory? old, GameLevel? previewLevel,
        DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiLevelCategoryResponse
        {
            Name = old.Name,
            Description = old.Description,
            IconHash = old.IconHash,
            FontAwesomeIcon = old.FontAwesomeIcon,
            ApiRoute = old.ApiRoute,
            RequiresUser = old.RequiresUser,
            PreviewLevel = ApiGameLevelResponse.FromOld(previewLevel, dataContext),
            Hidden = old.Hidden,
        };
    }
    
    public static ApiLevelCategoryResponse? FromOld(GameLevelCategory? old, DataContext dataContext) => FromOld(old, null, dataContext);

    public static IEnumerable<ApiLevelCategoryResponse> FromOldList(IEnumerable<GameLevelCategory> oldList,
        DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
    
    public static IEnumerable<ApiLevelCategoryResponse> FromOldList(IEnumerable<GameLevelCategory> oldList,
        RequestContext context,
        DataContext dataContext)
    {
        return oldList.Select(category =>
        {
            DatabaseList<GameLevel>? list = category.Fetch(context, 0, 1, dataContext, LevelFilterSettings.FromApiRequest(context), dataContext.User);
            GameLevel? level = list?.Items.FirstOrDefault();
            
            return FromOld(category, level, dataContext);
        }).ToList()!;
    }
}