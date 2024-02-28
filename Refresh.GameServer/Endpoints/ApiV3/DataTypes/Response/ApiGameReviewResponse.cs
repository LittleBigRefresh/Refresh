using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Reviews;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

public class ApiGameReviewResponse : IDataConvertableFrom<ApiGameReviewResponse, GameReview>
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
            ReviewId = old.SequentialId,
            Level = ApiGameLevelResponse.FromOld(old.Level.First())!,
            Publisher = ApiGameUserResponse.FromOld(old.Publisher)!,
            PostedAt = DateTimeOffset.FromUnixTimeMilliseconds(old.Timestamp),
            Labels = old.Labels,
            Text = old.Text,
        };
    }

    public static IEnumerable<ApiGameReviewResponse> FromOldList(IEnumerable<GameReview> oldList) => oldList.Select(FromOld).ToList()!;
}