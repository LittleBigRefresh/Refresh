using Refresh.Core.Types.Categories;
using Refresh.Core.Types.Data;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Categories;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiCategoryResponse : IApiResponse, IDataConvertableFrom<ApiCategoryResponse, GameCategory>
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string IconHash { get; set; }
    public required string FontAwesomeIcon { get; set; }
    public required string ApiRoute { get; set; }
    public required bool RequiresUser { get; set; }
    public required bool Hidden { get; set; } = false;
    
    public static ApiCategoryResponse? FromOld(GameCategory? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiCategoryResponse
        {
            Name = old.Name,
            Description = old.Description,
            IconHash = old.IconHash,
            FontAwesomeIcon = old.FontAwesomeIcon,
            ApiRoute = old.ApiRoute,
            RequiresUser = old.RequiresUser,
            Hidden = old.Hidden,
        };
    }

    public static IEnumerable<ApiCategoryResponse> FromOldList(IEnumerable<GameCategory> oldList, DataContext dataContext) 
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}