using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Levels;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Users;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Users.Photos;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Data;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Activity;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiActivityPageResponse : IApiResponse, IDataConvertableFrom<ApiActivityPageResponse, ActivityPage>
{
    public required IEnumerable<ApiEventResponse> Events { get; set; }
    public required IEnumerable<ApiGameUserResponse> Users { get; set; }
    public required IEnumerable<ApiGameLevelResponse> Levels { get; set; }
    public required IEnumerable<ApiGameScoreResponse> Scores { get; set; }
    public required IEnumerable<ApiGamePhotoResponse> Photos { get; set; }

    public static ApiActivityPageResponse Empty => new()
    {
        Events = [],
        Levels = [],
        Photos = [],
        Scores = [],
        Users = [],
    };
    
    public static ApiActivityPageResponse? FromOld(ActivityPage? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiActivityPageResponse
        {
            Events = ApiEventResponse.FromOldList(old.Events, dataContext),
            Users = ApiGameUserResponse.FromOldList(old.Users, dataContext),
            Levels = ApiGameLevelResponse.FromOldList(old.Levels, dataContext),
            Scores = ApiGameScoreResponse.FromOldList(old.Scores, dataContext),
            Photos = ApiGamePhotoResponse.FromOldList(old.Photos, dataContext),
        };
    }

    public static IEnumerable<ApiActivityPageResponse> FromOldList(IEnumerable<ActivityPage> oldList,
        DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}