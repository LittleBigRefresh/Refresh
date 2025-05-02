using Bunkum.Core;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types.Categories.Levels;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Levels;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiLevelCategoryResponse : IApiResponse, IDataConvertableFrom<ApiLevelCategoryResponse, GameLevelCategory>
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string IconHash { get; set; }
    public required string FontAwesomeIcon { get; set; }
    public required string ApiRoute { get; set; }
    public required bool RequiresUser { get; set; }
    public required ApiGameLevelResponse? PreviewLevel { get; set; }
    public required bool Hidden { get; set; } = false;
    
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
            DatabaseList<GameLevel>? list = category.Fetch(context, 0, 1, dataContext, new LevelFilterSettings(context, TokenGame.Website), dataContext.User);
            GameLevel? level = list?.Items.FirstOrDefault();
            
            return FromOld(category, level, dataContext);
        }).ToList()!;
    }
}