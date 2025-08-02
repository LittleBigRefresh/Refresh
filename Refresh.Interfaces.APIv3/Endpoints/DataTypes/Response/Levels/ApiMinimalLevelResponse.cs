using Refresh.Core.Types.Data;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Interfaces.APIv3.Extensions;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Levels;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiMinimalLevelResponse : IApiResponse, IDataConvertableFrom<ApiMinimalLevelResponse, GameLevel>
{
    public required int LevelId { get; set; }
    public required string Title { get; set; }
    public required string IconHash { get; set; }

    public static ApiMinimalLevelResponse? FromOld(GameLevel? level, DataContext dataContext)
    {
        if (level == null) return null;

        return new ApiMinimalLevelResponse
        {
            LevelId = level.LevelId,
            Title = level.Title,
            IconHash = level.GetIconHash(dataContext),
        };
    }

    public static IEnumerable<ApiMinimalLevelResponse> FromOldList(IEnumerable<GameLevel> oldList, DataContext dataContext)
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}