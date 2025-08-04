using Refresh.Core.Types.Data;
using Refresh.Database.Models.Comments;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Data;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Levels;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Comments;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiLevelCommentResponse : IApiResponse, IDataConvertableFrom<ApiLevelCommentResponse, GameLevelComment>
{
    public required int CommentId { get; set; }
    public required string Content { get; set; }
    public required ApiMinimalUserResponse Publisher { get; set; }
    public required ApiMinimalLevelResponse Level { get; set; }
    public required ApiRatingResponse Rating { get; set; }
    public required DateTimeOffset Timestamp { get; set; }
    
    public static ApiLevelCommentResponse? FromOld(GameLevelComment? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiLevelCommentResponse
        {
            CommentId = old.SequentialId,
            Content = old.Content,
            Publisher = ApiMinimalUserResponse.FromOld(old.Author, dataContext)!,
            Level = ApiMinimalLevelResponse.FromOld(old.Level, dataContext)!,
            Rating = ApiRatingResponse.FromRating
            (
                dataContext.Database.GetTotalRatingsForLevelComment(old, RatingType.Yay),
                dataContext.Database.GetTotalRatingsForLevelComment(old, RatingType.Boo),
                dataContext.User != null ? (int?)dataContext.Database.GetLevelCommentRatingByUser(old, dataContext.User) : 0
            ),
            Timestamp = old.Timestamp,
        };
    }
    
    public static IEnumerable<ApiLevelCommentResponse> FromOldList(IEnumerable<GameLevelComment> oldList, DataContext dataContext)
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}