using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameScoreResponse : IApiResponse, IDataConvertableFrom<ApiGameScoreResponse, GameSubmittedScore>
{
    public required string ScoreId { get; set; }
    public required ApiGameLevelResponse Level { get; set; }
    public required IEnumerable<ApiGameUserResponse> Players { get; set; }
    public required DateTimeOffset ScoreSubmitted { get; set; }
    public required int Score { get; set; }
    public required byte ScoreType { get; set; }
    
    public required TokenGame Game { get; set; }
    public required TokenPlatform Platform { get; set; }
    
    public static ApiGameScoreResponse? FromOld(GameSubmittedScore? old)
    {
        if (old == null) return null;

        return new ApiGameScoreResponse
        {
            ScoreId = old.ScoreId.ToString()!,
            Level = ApiGameLevelResponse.FromOld(old.Level)!,
            Players = ApiGameUserResponse.FromOldList(old.Players),
            ScoreSubmitted = old.ScoreSubmitted,
            Score = old.Score,
            ScoreType = old.ScoreType,
            Game = old.Game,
            Platform = old.Platform,
        };
    }

    public static IEnumerable<ApiGameScoreResponse> FromOldList(IEnumerable<GameSubmittedScore> oldList) => oldList.Select(FromOld)!;
}