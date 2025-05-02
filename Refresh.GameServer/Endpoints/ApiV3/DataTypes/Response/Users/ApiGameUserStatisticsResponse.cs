using Refresh.GameServer.Types.Data;
using Refresh.Database.Models.Users;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Users;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameUserStatisticsResponse: IApiResponse, IDataConvertableFrom<ApiGameUserStatisticsResponse, GameUser>
{
    public required int Favourites { get; set; }
    public required int ProfileComments { get; set; }
    public required int PublishedLevels { get; set; }
    public required int PhotosTaken { get; set; }
    
    public static ApiGameUserStatisticsResponse? FromOld(GameUser? user, DataContext dataContext)
    {
        if (user == null) return null;
        
        return new ApiGameUserStatisticsResponse
        {
            Favourites = dataContext.Database.GetTotalUsersFavouritingUser(user),
            ProfileComments = dataContext.Database.GetTotalCommentsForProfile(user),
            PublishedLevels = dataContext.Database.GetTotalLevelsByUser(user),
            PhotosTaken = dataContext.Database.GetTotalPhotosByUser(user),
        };
    }
    
    public static IEnumerable<ApiGameUserStatisticsResponse> FromOldList(IEnumerable<GameUser> oldList, DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}