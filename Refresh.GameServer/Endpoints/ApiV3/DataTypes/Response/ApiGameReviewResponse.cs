using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Reviews;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameReviewResponse : IApiResponse, IDataConvertableFrom<ApiGameReviewResponse, GameReview>
{
    public required int ReviewId { get; set; }
    public required ApiGameLevelResponse Level { get; set; }
    public required ApiGameUserResponse Publisher { get; set; }
    public required DateTimeOffset PostedAt { get; set; }
    public required string Labels { get; set; }
    public required string Text { get; set; }
    public static ApiGameReviewResponse? FromOld(GameReview? old)
    {
        if (old == null) return null;
        return new ApiGameReviewResponse
        {
            ReviewId = old.ReviewId,
            Level = ApiGameLevelResponse.FromOld(old.Level)!,
            Publisher = ApiGameUserResponse.FromOld(old.Publisher)!,
            PostedAt = old.PostedAt,
            Labels = old.Labels,
            Text = old.Content,
        };
    }

    public static IEnumerable<ApiGameReviewResponse> FromOldList(IEnumerable<GameReview> oldList) => oldList.Select(FromOld).ToList()!;
}