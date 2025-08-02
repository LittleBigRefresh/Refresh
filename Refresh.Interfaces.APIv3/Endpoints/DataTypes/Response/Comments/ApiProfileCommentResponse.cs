using Refresh.Core.Types.Data;
using Refresh.Database.Models.Comments;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Comments;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiProfileCommentResponse : IApiResponse, IDataConvertableFrom<ApiProfileCommentResponse, GameProfileComment>
{
    public required int CommentId { get; set; }
    public required string Content { get; set; }
    public required ApiMinimalUserResponse Poster { get; set; }
    public required ApiMinimalUserResponse Profile { get; set; }
    public required int YayRatings { get; set; }
    public required int BooRatings { get; set; }
    public required int OwnRating { get; set; }
    public required DateTimeOffset Timestamp { get; set; }
    
    public static ApiProfileCommentResponse? FromOld(GameProfileComment? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiProfileCommentResponse
        {
            CommentId = old.SequentialId,
            Content = old.Content,
            Poster = ApiMinimalUserResponse.FromOld(old.Author, dataContext)!,
            Profile = ApiMinimalUserResponse.FromOld(old.Profile, dataContext)!,
            YayRatings = dataContext.Database.GetTotalRatingsForProfileComment(old, RatingType.Yay),
            BooRatings = dataContext.Database.GetTotalRatingsForProfileComment(old, RatingType.Boo),
            OwnRating = dataContext.User != null ? (int?)dataContext.Database.GetProfileCommentRatingByUser(old, dataContext.User) ?? 0 : 0,
            Timestamp = old.Timestamp,
        };
    }
    
    public static IEnumerable<ApiProfileCommentResponse> FromOldList(IEnumerable<GameProfileComment> oldList, DataContext dataContext)
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}