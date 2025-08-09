using Bunkum.Core;
using Refresh.Core.Types.Categories.Users;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Categories;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiUserCategoryResponse : ApiCategoryResponse, IApiResponse, IDataConvertableFrom<ApiUserCategoryResponse, GameUserCategory>
{
    public required ApiGameUserResponse? PreviewItem { get; set; }
    
    public static ApiUserCategoryResponse? FromOld(GameUserCategory? old, GameUser? PreviewItem,
        DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiUserCategoryResponse
        {
            Name = old.Name,
            Description = old.Description,
            IconHash = old.IconHash,
            FontAwesomeIcon = old.FontAwesomeIcon,
            ApiRoute = old.ApiRoute,
            RequiresUser = old.RequiresUser,
            PreviewItem = ApiGameUserResponse.FromOld(PreviewItem, dataContext),
            Hidden = old.Hidden,
        };
    }
    
    public static ApiUserCategoryResponse? FromOld(GameUserCategory? old, DataContext dataContext) 
        => FromOld(old, null, dataContext);

    public static IEnumerable<ApiUserCategoryResponse> FromOldList(IEnumerable<GameUserCategory> oldList, DataContext dataContext) 
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
    
    public static IEnumerable<ApiUserCategoryResponse> FromOldList(IEnumerable<GameUserCategory> oldList,
        RequestContext context,
        DataContext dataContext)
    {
        return oldList.Select(category =>
        {
            DatabaseList<GameUser>? list = category.Fetch(context, 0, 1, dataContext, dataContext.User);
            GameUser? item = list?.Items.FirstOrDefault();
            
            return FromOld(category, item, dataContext);
        }).ToList()!;
    }
}