using Bunkum.Core.Storage;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Activity;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiActivityPageResponse : IApiResponse, IDataConvertableFrom<ApiActivityPageResponse, ActivityPage>
{
    public required IEnumerable<ApiEventResponse> Events { get; set; }
    public required IEnumerable<ApiGameUserResponse> Users { get; set; }
    public required IEnumerable<ApiGameLevelResponse> Levels { get; set; }
    public required IEnumerable<ApiGameScoreResponse> Scores { get; set; }
    
    public static ApiActivityPageResponse? FromOld(ActivityPage? old)
    {
        if (old == null) return null;

        return new ApiActivityPageResponse
        {
            Events = ApiEventResponse.FromOldList(old.Events),
            Users = ApiGameUserResponse.FromOldList(old.Users),
            Levels = ApiGameLevelResponse.FromOldList(old.Levels),
            Scores = ApiGameScoreResponse.FromOldList(old.Scores),
        };
    }

    public void FillInExtraData(GameDatabaseContext database, IDataStore dataStore)
    {
        foreach (ApiGameScoreResponse score in this.Scores) score.FillInExtraData(database, dataStore);
        foreach (ApiGameUserResponse user in this.Users) user.FillInExtraData(database, dataStore);
        foreach (ApiGameLevelResponse level in this.Levels) level.FillInExtraData(database, dataStore);
    }
    
    public static ApiActivityPageResponse? FromOldWithExtraData(ActivityPage? old, GameDatabaseContext database, IDataStore dataStore)
    {
        if (old == null) return null;

        ApiActivityPageResponse response = FromOld(old)!;
        response.FillInExtraData(database, dataStore);

        return response;
    }

    public static IEnumerable<ApiActivityPageResponse> FromOldList(IEnumerable<ActivityPage> oldList) => oldList.Select(FromOld).ToList()!;
}