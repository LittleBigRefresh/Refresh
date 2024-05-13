using Bunkum.Core.Storage;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Data;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiActivityPageResponse : IApiResponse, IDataConvertableFrom<ApiActivityPageResponse, ActivityPage>
{
    public required IEnumerable<ApiEventResponse> Events { get; set; }
    public required IEnumerable<ApiGameUserResponse> Users { get; set; }
    public required IEnumerable<ApiGameLevelResponse> Levels { get; set; }
    public required IEnumerable<ApiGameScoreResponse> Scores { get; set; }
    
    public static ApiActivityPageResponse? FromOld(ActivityPage? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiActivityPageResponse
        {
            Events = ApiEventResponse.FromOldList(old.Events, dataContext),
            Users = ApiGameUserResponse.FromOldList(old.Users, dataContext),
            Levels = ApiGameLevelResponse.FromOldList(old.Levels, dataContext),
            Scores = ApiGameScoreResponse.FromOldList(old.Scores, dataContext),
        };
    }

    public void FillInExtraData(GameDatabaseContext database, IDataStore dataStore)
    {
        foreach (ApiGameScoreResponse score in this.Scores) score.FillInExtraData(database, dataStore);
        foreach (ApiGameUserResponse user in this.Users) user.FillInExtraData(database, dataStore);
        foreach (ApiGameLevelResponse level in this.Levels) level.FillInExtraData(database, dataStore);
    }
    
    public static ApiActivityPageResponse? FromOldWithExtraData(ActivityPage? old, GameDatabaseContext database,
        IDataStore dataStore, DataContext dataContext)
    {
        if (old == null) return null;

        ApiActivityPageResponse response = FromOld(old, dataContext)!;
        response.FillInExtraData(database, dataStore);

        return response;
    }

    public static IEnumerable<ApiActivityPageResponse> FromOldList(IEnumerable<ActivityPage> oldList,
        DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}