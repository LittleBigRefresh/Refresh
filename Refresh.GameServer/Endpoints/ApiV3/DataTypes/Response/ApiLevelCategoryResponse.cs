using Bunkum.HttpServer;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Levels.Categories;
using Refresh.GameServer.Types.UserData;

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
    public required ApiGameLevelResponse? PreviewLevel { get; set; }
    
    public static ApiLevelCategoryResponse? FromOld(LevelCategory? old, GameLevel? previewLevel)
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
            PreviewLevel = ApiGameLevelResponse.FromOld(previewLevel),
        };
    }
    
    public static ApiLevelCategoryResponse? FromOld(LevelCategory? old) => FromOld(old, null);

    public static IEnumerable<ApiLevelCategoryResponse> FromOldList(IEnumerable<LevelCategory> oldList) => oldList.Select(FromOld)!;
    public static IEnumerable<ApiLevelCategoryResponse> FromOldList(IEnumerable<LevelCategory> oldList,
        RequestContext context,
        GameDatabaseContext database,
        GameUser? user)
    {
        return oldList.Select(category =>
        {
            DatabaseList<GameLevel>? list = category.Fetch(context, 0, 1, database, user);
            GameLevel? level = list?.Items.FirstOrDefault();
            
            return FromOld(category, level);
        })!;
    }
}