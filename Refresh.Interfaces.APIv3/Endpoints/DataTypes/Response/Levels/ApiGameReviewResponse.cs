using Refresh.Core.Types.Data;
using Refresh.Database.Models;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Levels;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Data;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Levels;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameReviewResponse : IApiResponse, IDataConvertableFrom<ApiGameReviewResponse, GameReview>
{
    public required int ReviewId { get; set; }
    public required ApiGameLevelResponse Level { get; set; }
    public required ApiGameUserResponse Publisher { get; set; }
    public required DateTimeOffset PostedAt { get; set; }
    public required List<Label> LabelList { get; set; }
    public required string Labels { get; set; } // TODO: remove this and rename LabelList to Labels in APIv4
    public required string Text { get; set; }
    public required ApiRatingResponse Rating { get; set; }
    
    public static ApiGameReviewResponse? FromOld(GameReview? old, DataContext dataContext)
    {
        if (old == null) return null;

        DatabaseRating rating = dataContext.Database.GetRatingForReview(old);

        return new ApiGameReviewResponse
        {
            ReviewId = old.ReviewId,
            Level = ApiGameLevelResponse.FromOld(old.Level, dataContext)!,
            Publisher = ApiGameUserResponse.FromOld(old.Publisher, dataContext)!,
            PostedAt = old.PostedAt,
            LabelList = old.Labels,
            Labels = old.Labels.ToLbpCommaList(),
            Text = old.Content,
            Rating = ApiRatingResponse.FromRating
            (
                rating.PositiveRating,
                rating.NegativeRating,
                dataContext.User != null ? (int?)dataContext.Database.GetRateReviewRelationForReview(dataContext.User, old)?.RatingType : 0
            )
        };
    }

    public static IEnumerable<ApiGameReviewResponse> FromOldList(IEnumerable<GameReview> oldList,
        DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}