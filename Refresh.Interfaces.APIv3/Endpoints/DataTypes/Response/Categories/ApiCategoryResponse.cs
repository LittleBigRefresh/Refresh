using Bunkum.Core;
using Refresh.Core.Types.Categories;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Categories;
using Refresh.Database.Query;

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

    public required IResultType? PreviewItem { get; set; }
    public required IResultType? PreviewLevel { get; set; } // Only here to not break APIv3 clients

    public static ApiCategoryResponse? FromOld(GameCategory? old, DataContext dataContext)
        => FromOld(old, null, dataContext);

    public static ApiCategoryResponse? FromOld(GameCategory? old, IResultType? preview, DataContext dataContext)
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
            PreviewItem = preview,
            PreviewLevel = preview,
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
            IResultType? preview = list?.Levels?.Items.FirstOrDefault();
            preview ??= list?.Users?.Items.FirstOrDefault();
            preview ??= list?.Playlists?.Items.FirstOrDefault();

            return FromOld(category, preview, dataContext);
        }).ToList()!;
    }
}