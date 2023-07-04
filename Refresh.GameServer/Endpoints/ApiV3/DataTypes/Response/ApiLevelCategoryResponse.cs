using Refresh.GameServer.Types.Levels.Categories;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiLevelCategoryResponse : IApiResponse, IDataConvertableFrom<ApiLevelCategoryResponse, LevelCategory>
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string IconHash { get; set; }
    public required string FontAwesomeIcon { get; set; }
    public required string ApiRoute { get; set; }
    public required bool RequiresUser { get; set; }
    
    public static ApiLevelCategoryResponse? FromOld(LevelCategory? old)
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
        };
    }

    public static IEnumerable<ApiLevelCategoryResponse> FromOldList(IEnumerable<LevelCategory> oldList) => oldList.Select(FromOld)!;
}