using System.Diagnostics;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Users;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;

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
        
        if(user.Statistics == null)
            dataContext.Database.RecalculateUserStatistics(user);
        
        Debug.Assert(user.Statistics != null);
        
        return new ApiGameUserStatisticsResponse
        {
            Favourites = user.Statistics.FavouriteCount,
            ProfileComments = user.Statistics.CommentCount,
            PublishedLevels = user.Statistics.LevelCount,
            PhotosTaken = user.Statistics.PhotosByUserCount,
        };
    }
    
    public static IEnumerable<ApiGameUserStatisticsResponse> FromOldList(IEnumerable<GameUser> oldList, DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}