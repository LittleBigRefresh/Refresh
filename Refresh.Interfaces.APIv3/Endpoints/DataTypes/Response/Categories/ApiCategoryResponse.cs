using Bunkum.Core;
using Refresh.Core.Types.Categories;
using Refresh.Core.Types.Data;
using Refresh.Database.Query;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Levels;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;

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

    // Source: https://stackoverflow.com/questions/2254872/using-json-net-converters-to-deserialize-properties/6303853#6303853
    // This is to fix Newtonsoft not being able to deserialize these attributes in unit tests
    [JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
    public IApiResultResponse? PreviewItem { get; set; } = null;

    [JsonProperty( TypeNameHandling = TypeNameHandling.Objects )]
    public IApiResultResponse? PreviewLevel { get; set; } = null; // TODO: Remove and use PreviewItem for levels aswell in APIv4

    public static ApiCategoryResponse? FromOld(GameCategory? old, DataContext dataContext)
        => FromOld(old, null, dataContext);

    public static ApiCategoryResponse? FromOld(GameCategory? old, IApiResultResponse? preview, DataContext dataContext)
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
            // Save on response size
            PreviewItem = old.PrimaryResultType != ResultType.Level ? preview : null,
            PreviewLevel = old.PrimaryResultType == ResultType.Level ? preview : null,
        };
    }

    public static IEnumerable<ApiCategoryResponse> FromOldList(IEnumerable<GameCategory> oldList, DataContext dataContext) 
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
    
    public static IEnumerable<ApiCategoryResponse> FromOldList(IEnumerable<GameCategory> oldList,
        RequestContext context,
        DataContext dataContext)
    {
        return oldList.Select(category =>
        {
            DatabaseResultList? list = category.Fetch(context, 0, 1, dataContext, LevelFilterSettings.FromApiRequest(context), dataContext.User);

            // Take a preview, preferring levels over users over playlists
            IApiResultResponse? preview = ApiGameLevelResponse.FromOld(list?.Levels?.Items.FirstOrDefault(), dataContext);
            preview ??= ApiGameUserResponse.FromOld(list?.Users?.Items.FirstOrDefault(), dataContext);

            return FromOld(category, preview, dataContext);
        }).ToList()!;
    }
}